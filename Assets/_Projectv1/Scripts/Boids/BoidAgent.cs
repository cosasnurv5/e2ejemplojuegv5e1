using System.Collections.Generic;
using UnityEngine;
using Steering;
using FSM;
using Boids.States;
using Zones; 

namespace Boids
{
    [RequireComponent(typeof(BoidSensor))]
    public class BoidAgent : SteeringAgent
    {
        public static readonly List<BoidAgent> allBoids = new List<BoidAgent>();

        private void OnEnable()
        {
            if (!allBoids.Contains(this))
                allBoids.Add(this);
        }

        private void OnDisable()
        {
            allBoids.Remove(this);
        }

        [Header("Rangos de Percepción.")]
        public float neighborRadius = 5f;
        public float separationRadius = 3.5f;
        public float visionRadius = 8f;

        [Header("Pesos de Comportamiento.")]
        public float separationWeight = 10.0f;
        public float alignmentWeight = 1.0f;
        public float cohesionWeight = 0.7f;
        public float evadeWeight = 4.0f;
        public float arriveWeight = 1.5f;

        [Header("Zonv.")]
        public LayerMask huntZoneLayer;
        public Vector3 huntZoneSize = new Vector3(15f, 5f, 15f);

        [Header("WallLayer.")]
        public LayerMask wallLayer;
        public float wallAvoidanceDistance = 3.0f;
        public float wallAvoidanceWeight = 6.0f;

        [Header("Configuración.")]
        public int HitsReceived { get; private set; } = 0;
        public float damagePerSecond = 25f;
        public float interactDistance = 1.2f;

        public bool IsDestroyed { get; set; }
        public Transform CurrentInterestTarget { get; set; }

        public BoidSensor Sensor { get; private set; }
        public FiniteStateMachine FSM { get; private set; }

        public BoidFlockngState FlockngState { get; private set; }
        public BoidEvadState EvadState { get; private set; }
        public BoidSkObjState SkObjState { get; private set; }
        public BoidIntrState IntrState { get; private set; }
        public BoidDestroydState DestroydState { get; private set; }
        public BoidRespwnState RespwnState { get; private set; }

        protected void Awake()
        {
            Sensor = GetComponent<BoidSensor>();
            FSM = new FiniteStateMachine();

            FlockngState = new BoidFlockngState(this);
            EvadState = new BoidEvadState(this);
            SkObjState = new BoidSkObjState(this);
            IntrState = new BoidIntrState(this);
            DestroydState = new BoidDestroydState(this);
            RespwnState = new BoidRespwnState(this);
        }

        private void Start()
        {
            FSM.ChangeState(FlockngState);
        }

        protected override void Update()
        {
            if (IsDestroyed && FSM.CurrentState != DestroydState && FSM.CurrentState != RespwnState)
            {
                FSM.ChangeState(DestroydState);
            }

            if (Managers.GameManager.Instance != null)
            {
                huntZoneSize = Managers.GameManager.Instance.huntZoneSize;
            }

           
            Vector3 wallForce = CalculateWallAvoidance() * wallAvoidanceWeight;
            AddForce(wallForce);

         
            Vector3 center = GetHuntZoneCenter();
            if (center != Vector3.zero)
            {
                Vector3 pos = transform.position;
                float halfX = huntZoneSize.x / 2f;
                float halfZ = huntZoneSize.z / 2f;

                bool isOutside = (pos.x < center.x - halfX || pos.x > center.x + halfX ||
                                  pos.z < center.z - halfZ || pos.z > center.z + halfZ);

                if (isOutside)
                {
                    pos.x = Mathf.Clamp(pos.x, center.x - halfX + 0.5f, center.x + halfX - 0.5f);
                    pos.z = Mathf.Clamp(pos.z, center.z - halfZ + 0.5f, center.z + halfZ - 0.5f);
                    transform.position = pos;

                    Velocity = -Velocity * 0.8f;
                }
            }

            FSM.Update();
            base.Update();
        }

        public Vector3 GetHuntZoneCenter()
        {
           
            if (HunterZonv.Instance != null)
            {
                return HunterZonv.Instance.transform.position;
            }
            return Vector3.zero;
        }

        public Vector3 CalculateWallAvoidance()
        {
            Vector3 avoidanceForce = Vector3.zero;
            Vector3[] directions = { transform.forward, (transform.forward + transform.right).normalized, (transform.forward - transform.right).normalized };

            foreach (var dir in directions)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, wallAvoidanceDistance, wallLayer))
                {
                    Vector3 desired = hit.normal * maxSpeed;
                    avoidanceForce += (desired - Velocity);
                }
            }

            return avoidanceForce;
        }

        public void ReceiveHit(int hits)
        {
            if (IsDestroyed) return;

            HitsReceived += hits;

            if (HitsReceived >= 2)
            {
                HitsReceived = 2;
                IsDestroyed = true;
                FSM.ChangeState(DestroydState);
            }
        }

        public void ApplyFlockingForces()
        {
            List<BoidAgent> neighbors = Sensor.GetNeighbors(neighborRadius);

            if (neighbors.Count > 0)
            {
                AddForce(CalculateSeparation(neighbors) * separationWeight);
                AddForce(CalculateAlignment(neighbors) * alignmentWeight);
                AddForce(CalculateCohesion(neighbors) * cohesionWeight);
            }
            else
            {
                Vector3 wanderForce = new Vector3(Mathf.Sin(Time.time + transform.GetInstanceID()), 0f, Mathf.Cos(Time.time + transform.GetInstanceID())) * (maxSpeed * 0.5f);
                AddForce(wanderForce);
            }

            if (Velocity.sqrMagnitude < 0.2f)
            {
                AddForce(transform.forward * maxSpeed);
            }
        }

        public Vector3 CalculateSeparation(List<BoidAgent> neighbors)
        {
            Vector3 separation = Vector3.zero;
            int count = 0;
            float sqrSeparationRadius = separationRadius * separationRadius;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor == this || neighbor.IsDestroyed) continue;

                Vector3 diff = transform.position - neighbor.transform.position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < sqrSeparationRadius)
                {
                    if (sqrDist < 0.001f)
                    {
                        separation += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * 10f;
                    }
                    else
                    {
                        separation += diff.normalized / sqrDist;
                    }
                    count++;
                }
            }

            if (count > 0) separation /= count;
            return separation;
        }

        public Vector3 CalculateAlignment(List<BoidAgent> neighbors)
        {
            Vector3 averageVelocity = Vector3.zero;
            int validNeighbors = 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.IsDestroyed) continue;
                averageVelocity += neighbor.Velocity;
                validNeighbors++;
            }

            if (validNeighbors == 0) return Vector3.zero;

            averageVelocity /= validNeighbors;
            return averageVelocity - Velocity;
        }

        public Vector3 CalculateCohesion(List<BoidAgent> neighbors)
        {
            Vector3 centerOfMass = Vector3.zero;
            int validNeighbors = 0;

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null || neighbor.IsDestroyed) continue;
                centerOfMass += neighbor.transform.position;
                validNeighbors++;
            }

            if (validNeighbors == 0) return Vector3.zero;

            centerOfMass /= validNeighbors;
            return SteeringBehaviors.Seek(this, centerOfMass);
        }

        public void StopAgent()
        {
            Velocity = Vector3.zero;
        }

        public void ResetAgent()
        {
            HitsReceived = 0;
            IsDestroyed = false;
            gameObject.SetActive(true);
            FSM.ChangeState(FlockngState);
        }
    }
}