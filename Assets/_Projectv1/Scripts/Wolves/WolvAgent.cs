using System.Collections.Generic;
using UnityEngine;
using Steering;
using FSM;
using Hunter;
using Wolves.States;
using Wolves;
using Zones;

[RequireComponent(typeof(WolvPercepv))]
public class WolvAgent : SteeringAgent
{
    [Header("Configuración.")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.0f;
    public float captureRadius = 1.2f;
    public float rSpeed = 3.5f;
    public WaypointPath patrolPath;

    [Header("WallLayer.")]
    public LayerMask wallLayer;
    public float wallAvoidanceDistance = 3.0f;
    public float wallAvoidanceWeight = 8.0f;

    [Header("RLobos")]
    public float separationRadius = 2.5f;
    public float separationWeight = 12.0f;

    [Header("HtrObjetivo.")]
    public HunterNPC targetHunter;
    public Vector3 lastKnownHunterPosition;
    public bool hasTargetPosition = false;

    private float ignoreHunterTimer = 0f;
    public Vector3 spawnPosition { get; private set; }

    public WolvPercepv Perception { get; private set; }
    public FiniteStateMachine FSM { get; private set; }
    public WolvChaseState ChaseState { get; private set; }
    public WolvPatrolState PatrolState { get; private set; }

    public List<GraphNode> currentPathNodes;
    public int currentPathIndex = 0;

    public static readonly List<WolvAgent> allWolves = new List<WolvAgent>();

    protected void Awake()
    {
        Perception = GetComponent<WolvPercepv>();
        FSM = new FiniteStateMachine();

        PatrolState = new WolvPatrolState(this);
        ChaseState = new WolvChaseState(this);
    }

    private void OnEnable() => allWolves.Add(this);
    private void OnDisable() => allWolves.Remove(this);

    private void Start()
    {
        spawnPosition = transform.position;
        FSM.ChangeState(PatrolState);
    }

    protected override void Update()
    {
        if (ignoreHunterTimer > 0f)
        {
            ignoreHunterTimer -= Time.deltaTime;
        }

        if (IsInsideSafeZone(transform.position))
        {
            targetHunter = null;
            hasTargetPosition = false;
            currentPathNodes = null;
            if (FSM.CurrentState != PatrolState)
            {
                FSM.ChangeState(PatrolState);
            }
            base.Update();
            return;
        }

        Vector3 wallForce = CalculateWallAvoidance() * wallAvoidanceWeight * 1.5f;

        Vector3 separationForce = Vector3.zero;
        if (!(FSM.CurrentState == PatrolState && IsAtStartWaypoint()))
        {
            separationForce = CalculateSeparation() * separationWeight;
        }

        AddForce(wallForce);
        AddForce(separationForce);

     
        if (targetHunter != null && (targetHunter.IsInSafeZone() || targetHunter.FSM.CurrentState == targetHunter.DestrydState || IsInsideSafeZone(targetHunter.transform.position)))
        {
            targetHunter = null;
            hasTargetPosition = false;
            lastKnownHunterPosition = Vector3.zero;
            ignoreHunterTimer = 2.0f;

            if (FSM.CurrentState != PatrolState)
            {
                FSM.ChangeState(PatrolState);
            }
        }

        
        if (targetHunter != null && targetHunter.FSM.CurrentState != targetHunter.DestrydState && !targetHunter.IsInSafeZone())
        {
            float distToHunter = Vector3.Distance(transform.position, targetHunter.transform.position);
            if (distToHunter <= captureRadius)
            {
                targetHunter.FSM.ChangeState(targetHunter.DestrydState);
                ignoreHunterTimer = 2.0f;
            }
        }

        FSM.Update();
        base.Update();

       
        if (ignoreHunterTimer <= 0f && (targetHunter == null || (!targetHunter.IsInSafeZone() && targetHunter.FSM.CurrentState != targetHunter.DestrydState)))
        {
            if (Perception.CanSeeHunter(out var hunter))
            {
                if (hunter != null && (hunter.IsInSafeZone() || hunter.FSM.CurrentState == hunter.DestrydState || IsInsideSafeZone(hunter.transform.position))) return;


                bool wasAlreadyTargeted = (targetHunter != null);

                targetHunter = hunter;
                lastKnownHunterPosition = hunter.transform.position;
                hasTargetPosition = true;


                if (!wasAlreadyTargeted)
                {
                    Debug.Log($"El lobo {gameObject.name} detectó al HunterNPC en {lastKnownHunterPosition}. Alertando.");
                    AlertNearbyWolves(lastKnownHunterPosition);
                }

                if (FSM.CurrentState != ChaseState)
                {
                    FSM.ChangeState(ChaseState);
                }
            }
        }

        if (FSM.CurrentState == PatrolState && Velocity.sqrMagnitude < 0.05f)
        {
            Vector3 randomUnstick = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * (patrolSpeed * 0.8f);
            AddForce(randomUnstick);
        }

       
        if (hasTargetPosition)
        {
            Debug.DrawLine(transform.position, lastKnownHunterPosition, Color.red);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }

    public bool IsAtStartWaypoint()
    {
        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatSpawn = new Vector3(spawnPosition.x, 0, spawnPosition.z);
        float dist = Vector3.Distance(flatCurrent, flatSpawn);

        return dist < 2.5f;
    }

    public static bool IsInsideSafeZone(Vector3 pos)
    {
        foreach (var safeZone in SafeZonv.allSafZonv)
        {
            if (safeZone != null)
            {
                Collider col = safeZone.GetComponent<Collider>();
                if (col != null)
                {
                    if (col.bounds.Contains(pos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public Vector3 CalculateWallAvoidance()
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Vector3[] directions = {
            transform.forward,
            (transform.forward + transform.right * 0.75f).normalized,
            (transform.forward - transform.right * 0.75f).normalized,
            (transform.forward + transform.right * 1.5f).normalized,
            (transform.forward - transform.right * 1.5f).normalized,
            transform.right,
            -transform.right
        };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, wallAvoidanceDistance * 1.4f, wallLayer))
            {
              
                float closenessMultiplier = 1f - (hit.distance / (wallAvoidanceDistance * 1.4f));
                Vector3 desired = hit.normal * maxSpeed * (5f + (closenessMultiplier * 8f));
                avoidanceForce += (desired - Velocity);
            }
        }

        return avoidanceForce;
    }

    public Vector3 CalculateSeparation()
    {
        Vector3 separationForce = Vector3.zero;
        int count = 0;
        float sqrSepRadius = separationRadius * separationRadius;

        foreach (var otherWolf in allWolves)
        {
            if (otherWolf != null && otherWolf != this)
            {
                Vector3 diff = transform.position - otherWolf.transform.position;
                float sqrDist = diff.sqrMagnitude;

                if (sqrDist < sqrSepRadius && sqrDist > 0.0001f)
                {
                    if (sqrDist < 0.001f)
                    {
                        separationForce += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * 10f;
                    }
                    else
                    {
                        separationForce += diff.normalized / sqrDist;
                    }
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separationForce /= count;
            separationForce = separationForce.normalized * maxSpeed - Velocity;
        }

        return separationForce;
    }

    public void AlertNearbyWolves(Vector3 pos)
    {
        foreach (var wolf in allWolves)
        {
            if (wolf != null && wolf != this)
            {
                wolf.ReceiveAlert(pos);
            }
        }
    }

    public void ReceiveAlert(Vector3 pos)
    {
        if (ignoreHunterTimer > 0f) return;

        if (targetHunter != null && (targetHunter.IsInSafeZone() || targetHunter.FSM.CurrentState == targetHunter.DestrydState || IsInsideSafeZone(pos))) return;

        lastKnownHunterPosition = pos;
        hasTargetPosition = true;


        if (FSM.CurrentState == PatrolState)
        {
            Debug.Log($"El lobo {gameObject.name} con la recepción de la alerta se dirige a la posición: {pos}");
            FSM.ChangeState(ChaseState);
        }


    }

    public void SetPath(List<GraphNode> path)
    {
        currentPathNodes = path;
        currentPathIndex = 0;
    }

    public void Stop()
    {
        Velocity = Vector3.zero;
    }
}