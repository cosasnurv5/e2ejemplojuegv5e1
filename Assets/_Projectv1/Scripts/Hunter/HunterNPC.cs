using UnityEngine;
using FSM;
using Steering;
using Boids;
using Hunter.States;
using Environment;
using Zones;
using System.Collections.Generic;

namespace Hunter
{
    public class HunterNPC : SteeringAgent
    {
        [Header("Requerimientos.")]
        public float TBA = 3f;
        public float RangeAttackRadius = 8f;
        public float MeleeAttackRadius = 2f;
        public int RequiredGathers = 3;
        public float RespawnTime = 5f;
        public float SafeZoneWaitTime = 3f;

        [Header("Velocidades.")]
        public float normalSpeed = 3.5f;

        [Header("Energía")]
        public float maxEnergy = 100f;
        public float Energy = 100f;
        public float energyD = 10f;
        public float energyR = 20f;

        [Header("Detección de Layer.")]
        public float visionRadius = 10f;
        public float wolfDetectionRadius = 12f;
        public LayerMask boidLayer;
        public LayerMask wolfLayer;
        public ObjectSpawner spawner;
        public float mapLimitX = 18f;
        public float mapLimitZ = 18f;

        [Header("Layer.")]
        public LayerMask initialZoneLayer;
        public LayerMask huntZoneLayer;
        public LayerMask exitZoneLayer;
        public LayerMask safeZoneLayer;

        [Header("Wall.")]
        public LayerMask wallLayer;
        public float wallAvoidanceDistance = 3.5f;
        public float wallAvoidanceWeight = 25f;

        [Header("ContadoresBoid.")]
        public int CollectedCount = 0;

        public List<GraphNode> currentPath { get; private set; }
        public int currentPathIndex = 0;

        public FiniteStateMachine FSM { get; private set; }

        public HunterSearchState SearchState { get; private set; }
        public HunterAttackState AttackState { get; private set; }
        public HunterRvState RvState { get; private set; }
        public HunterGatherState GatherState { get; private set; }
        public HunterDervState DervState { get; private set; }
        public HunterExtState ExtState { get; private set; }
        public HunterDestrydState DestrydState { get; private set; }

        public float TBATimer { get; set; }
        public BoidAgent CurrentTarget { get; set; }
        public BoidAgent DestroyedTarget { get; set; }

        private bool isInSafeZone = false;
        private bool isInExitZone = false;
        private bool isInInitialZone = false;

        protected void Awake()
        {
            FSM = new FiniteStateMachine();

            SearchState = new HunterSearchState(this);
            AttackState = new HunterAttackState(this);
            GatherState = new HunterGatherState(this);
            RvState = new HunterRvState(this);
            DervState = new HunterDervState(this);
            ExtState = new HunterExtState(this);
            DestrydState = new HunterDestrydState(this);
        }

        private void Start()
        {
            TBATimer = 0f;
            TeleportToInitialZone();
            FSM.ChangeState(SearchState);
        }

        public Vector3 GetPositionByLayer(LayerMask layer)
        {
            if (layer == initialZoneLayer && InitialZonv.Instance != null)
                return InitialZonv.Instance.transform.position;
            if (layer == huntZoneLayer && HunterZonv.Instance != null)
                return HunterZonv.Instance.transform.position;
            if (layer == exitZoneLayer && ExtZonv.Instance != null)
                return ExtZonv.Instance.transform.position;

            return Vector3.zero;
        }

        protected override void Update()
        {
            
            if (TBATimer > 0)
            {
                TBATimer -= Time.deltaTime;
            }
            else if (IsInHuntZone())
            {
             
                if (spawner != null)
                {
               
                    Vector3 spawnPos = GetRandomPositionInHuntZone();

                 
                    spawner.SpawnInterestObject(spawnPos);

                    Debug.Log("El HunterNPC,Generando objeto en la HunterZonv.");
                }

               
                TBATimer = TBA;
            }

            if (FSM.CurrentState == DestrydState)
            {
                FSM.Update();
                return;
            }

           
            if (FSM.CurrentState != DervState && DetectWolf() && !IsInSafeZone())
            {
                FSM.ChangeState(DervState);
            }
           
            else if (CollectedCount >= RequiredGathers && FSM.CurrentState != ExtState && FSM.CurrentState != DervState)
            {
                FSM.ChangeState(ExtState);
            }

            Vector3 wallForce = CalculateWallAvoidance() * wallAvoidanceWeight;
            AddForce(wallForce);

            FSM.Update();
            base.Update();
        }

        private void OnTriggerEnter(Collider other)
        {
            int layerMaskVal = 1 << other.gameObject.layer;
            if ((layerMaskVal & safeZoneLayer) != 0) isInSafeZone = true;
            if ((layerMaskVal & exitZoneLayer) != 0) isInExitZone = true;
            if ((layerMaskVal & initialZoneLayer) != 0) isInInitialZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            int layerMaskVal = 1 << other.gameObject.layer;
            if ((layerMaskVal & safeZoneLayer) != 0) isInSafeZone = false;
            if ((layerMaskVal & exitZoneLayer) != 0) isInExitZone = false;
            if ((layerMaskVal & initialZoneLayer) != 0) isInInitialZone = false;
        }

        public Transform GetNrvSafZonv()
        {
            Transform bestZone = null;
            int minPathNodeCount = int.MaxValue;

            if (GraphManager.Instance == null) return null;

            GraphNode startNode = GraphManager.Instance.GetNearestNode(transform.position);
            if (startNode == null) return null;

            foreach (var safZonv in SafeZonv.allSafZonv)
            {
                if (safZonv == null) continue;

                GraphNode targetNode = GraphManager.Instance.GetNearestNode(safZonv.transform.position);
                if (targetNode == null) continue;

                var path = APathFinderv.FindPath(startNode, targetNode, false);
                if (path != null && path.Count < minPathNodeCount)
                {
                    minPathNodeCount = path.Count;
                    bestZone = safZonv.transform;
                }
            }

            return bestZone;
        }

        public Vector3 GetRandomPositionInHuntZone()
        {
            if (HunterZonv.Instance != null && Managers.GameManager.Instance != null)
            {
                Vector3 center = HunterZonv.Instance.transform.position;
                Vector3 size = Managers.GameManager.Instance.huntZoneSize;

                float randomX = Random.Range(-size.x / 2f + 2f, size.x / 2f - 2f);
                float randomZ = Random.Range(-size.z / 2f + 2f, size.z / 2f - 2f);

                return new Vector3(center.x + randomX, center.y, center.z + randomZ);
            }

           
            return transform.position + transform.forward * 3f;
        }

        public Vector3 CalculateWallAvoidance()
        {
            Vector3 avoidanceForce = Vector3.zero;
            Vector3 origin = transform.position + Vector3.up * 0.5f;

            Vector3[] directions = {
                transform.forward,
                (transform.forward + transform.right * 0.5f).normalized,
                (transform.forward - transform.right * 0.5f).normalized,
                (transform.forward + transform.right * 1.0f).normalized,
                (transform.forward - transform.right * 1.0f).normalized,
                transform.right,
                -transform.right
            };

            foreach (var dir in directions)
            {
                bool hitWall = Physics.Raycast(origin, dir, out RaycastHit hit, wallAvoidanceDistance, wallLayer);
                if (hitWall)
                {
                    float closenessMultiplier = 1f - (hit.distance / wallAvoidanceDistance);
                    Vector3 desired = hit.normal * maxSpeed * (3f + (closenessMultiplier * 5f));
                    avoidanceForce += (desired - Velocity);
                }
            }
            return avoidanceForce;
        }

        public bool DetectWolf()
        {
            foreach (var wolf in WolvAgent.allWolves)
            {
                if (wolf == null) continue;

                float sqrDist = (transform.position - wolf.transform.position).sqrMagnitude;
                if (sqrDist <= wolfDetectionRadius * wolfDetectionRadius)
                {
                    Vector3 dir = (wolf.transform.position - transform.position);
                    float dist = dir.magnitude;
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dir.normalized, dist, wallLayer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsInHuntZone()
        {
            if (HunterZonv.Instance != null && Managers.GameManager.Instance != null)
            {
                Vector3 center = HunterZonv.Instance.transform.position;
                Vector3 size = Managers.GameManager.Instance.huntZoneSize;
                Vector3 pos = transform.position;

                float halfX = size.x / 2f;
                float halfZ = size.z / 2f;

                return (pos.x >= center.x - halfX && pos.x <= center.x + halfX &&
                        pos.z >= center.z - halfZ && pos.z <= center.z + halfZ);
            }
            return false;
        }

        public bool IsTargetOutOfBounds(Vector3 position)
        {
            return Mathf.Abs(position.x) > mapLimitX || Mathf.Abs(position.z) > mapLimitZ;
        }

        public void SetPath(List<GraphNode> path)
        {
            currentPath = path;
            currentPathIndex = 0;
        }

        public Vector3 GetCurrentPathTarget()
        {
            if (currentPath == null || currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
                return transform.position;
            return currentPath[currentPathIndex].transform.position;
        }

        public void AdvancePath()
        {
            if (currentPath != null && currentPathIndex < currentPath.Count)
            {
                currentPathIndex++;
            }
        }

        public BoidAgent FindValidBoidInVision()
        {
            float sqrVision = visionRadius * visionRadius;
            foreach (var boid in BoidAgent.allBoids)
            {
                if (boid != null && !boid.IsDestroyed)
                {
                    float sqrDist = (transform.position - boid.transform.position).sqrMagnitude;
                    if (sqrDist <= sqrVision) return boid;
                }
            }
            return null;
        }

        public BoidAgent FindDestroyedBoidInVision()
        {
            float sqrVision = visionRadius * visionRadius;
            foreach (var boid in BoidAgent.allBoids)
            {
                if (boid != null && boid.IsDestroyed)
                {
                    float sqrDist = (transform.position - boid.transform.position).sqrMagnitude;
                    if (sqrDist <= sqrVision) return boid;
                }
            }
            return null;
        }

       
        public bool IsInSafeZone()
        {
            if (isInSafeZone) return true;

            foreach (var safeZone in SafeZonv.allSafZonv)
            {
                if (safeZone != null)
                {
                    Collider col = safeZone.GetComponent<Collider>();
                    if (col != null && col.bounds.Contains(transform.position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsAtExitZone() => isInExitZone;
        public bool IsInInitialZone() => isInInitialZone;

        public void TeleportToInitialZone()
        {
            Vector3 zonePos = GetPositionByLayer(initialZoneLayer);
            if (zonePos != Vector3.zero)
            {
                transform.position = zonePos;
            }
            else if (InitialZonv.Instance != null)
            {
                transform.position = InitialZonv.Instance.transform.position;
            }
            Stop();
        }

        public void Stop() => Velocity = Vector3.zero;

        private void OnDrawGizmos()
        {
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);

           
            if (currentPath != null && currentPathIndex < currentPath.Count && currentPath[currentPathIndex] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentPath[currentPathIndex].transform.position);
                Gizmos.DrawSphere(currentPath[currentPathIndex].transform.position, 0.4f);
            }
        }
    }
}