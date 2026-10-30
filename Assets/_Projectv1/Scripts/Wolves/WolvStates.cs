using UnityEngine;
using FSM;
using Steering;
using System.Collections.Generic;
using Zones;

namespace Wolves.States
{
    public class WolvPatrolState : IState
    {
        private readonly WolvAgent wolf;
        private const float WAYPOINT_THRESHOLD = 2.0f;
        public bool isReturningToStart = true;

        private float stuckTimer = 0f;
        private Vector3 lastPosition;

        public WolvPatrolState(WolvAgent wolf) => this.wolf = wolf;

        public void Enter()
        {
            wolf.maxSpeed = wolf.rSpeed;
            isReturningToStart = true;
            wolf.hasTargetPosition = false;
            wolf.targetHunter = null;
            wolf.currentPathNodes = null;
            wolf.currentPathIndex = 0;

            stuckTimer = 0f;
            lastPosition = wolf.transform.position;
        }

        public void Update()
        {
           
            if (Vector3.Distance(wolf.transform.position, lastPosition) < 0.15f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 2.2f)
                {
                   
                    wolf.currentPathNodes = null;
                    wolf.currentPathIndex = 0;

                    Vector3 escapeDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    wolf.AddForce(escapeDir * wolf.maxSpeed * 4f);

                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
                lastPosition = wolf.transform.position;
            }

        
            
          
            if (!isReturningToStart)
            {
         
                if (wolf.Perception.CanSeeHunter(out var hunter) && hunter != null && !hunter.IsInSafeZone() && hunter.FSM.CurrentState != hunter.DestrydState)
                {
                    wolf.targetHunter = hunter;
                    wolf.hasTargetPosition = true;
                    wolf.lastKnownHunterPosition = hunter.transform.position;

                    wolf.AlertNearbyWolves(wolf.lastKnownHunterPosition);
                    wolf.FSM.ChangeState(wolf.ChaseState);
                    return;
                }

               
                if (wolf.hasTargetPosition)
                {
                    wolf.FSM.ChangeState(wolf.ChaseState);
                    return;
                }
            }

          
            Vector3 targetPoint;
            if (isReturningToStart)
            {
                targetPoint = wolf.spawnPosition;
                wolf.maxSpeed = wolf.rSpeed;
            }
            else
            {
                wolf.maxSpeed = wolf.patrolSpeed;
                if (wolf.patrolPath != null && wolf.patrolPath.waypoints.Length > 0)
                {
                    targetPoint = wolf.patrolPath.GetCurrentWaypoint();
                }
                else
                {
                    targetPoint = wolf.spawnPosition;
                }
            }

            Vector3 flatWolf = new Vector3(wolf.transform.position.x, 0f, wolf.transform.position.z);
            Vector3 flatTargetPoint = new Vector3(targetPoint.x, 0f, targetPoint.z);
            float distToWaypoint = Vector3.Distance(flatWolf, flatTargetPoint);

            
            if (isReturningToStart && distToWaypoint < WAYPOINT_THRESHOLD)
            {
                wolf.Stop();

               
                isReturningToStart = false;
                wolf.maxSpeed = wolf.patrolSpeed;
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;
                return;
            }

           
            Vector3 dirToTarget = targetPoint - wolf.transform.position;
            float dist = dirToTarget.magnitude;
            bool hasLineOfSight = !Physics.Raycast(wolf.transform.position + Vector3.up * 0.5f, dirToTarget.normalized, dist, wolf.wallLayer);

            if (hasLineOfSight)
            {
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;

                if (isReturningToStart)
                {
                    wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPoint));
                }
                else
                {
                    if (dist > 3f)
                        wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPoint));
                    else
                        wolf.AddForce(SteeringBehaviors.Arrive(wolf, targetPoint, 2f));
                }
            }
            else
            {
                if (wolf.currentPathNodes == null || wolf.currentPathNodes.Count == 0)
                {
                    if (GraphManager.Instance != null)
                    {
                        GraphNode startNode = GraphManager.Instance.GetNearestNode(wolf.transform.position);
                        GraphNode targetNode = GraphManager.Instance.GetNearestNode(targetPoint);

                        if (startNode != null && targetNode != null)
                        {
                            wolf.currentPathNodes = APathFinderv.FindPath(startNode, targetNode, true);
                            wolf.currentPathIndex = 0;
                        }
                    }
                }

                if (wolf.currentPathNodes != null && wolf.currentPathNodes.Count > 0)
                {
                    if (wolf.currentPathIndex < wolf.currentPathNodes.Count)
                    {
                        Vector3 nodePos = wolf.currentPathNodes[wolf.currentPathIndex].transform.position;
                        Vector3 flatNode = new Vector3(nodePos.x, 0f, nodePos.z);
                        float nodeDist = Vector3.Distance(flatWolf, flatNode);

                        wolf.AddForce(SteeringBehaviors.Seek(wolf, nodePos));

                        bool reachedNode = nodeDist < WAYPOINT_THRESHOLD;
                        bool passedNode = false;

                        if (!reachedNode && wolf.Velocity.sqrMagnitude > 0.1f)
                        {
                            Vector3 dirToNode = (flatNode - flatWolf).normalized;
                            float dot = Vector3.Dot(wolf.Velocity.normalized, dirToNode);
                            if (dot < -0.3f && nodeDist < WAYPOINT_THRESHOLD * 2f)
                            {
                                passedNode = true;
                            }
                        }

                        if (reachedNode || passedNode)
                        {
                            wolf.currentPathIndex++;
                        }
                    }
                    else
                    {
                        wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPoint));
                    }
                }
                else
                {
                    wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPoint));
                }
            }

            if (!isReturningToStart && distToWaypoint < WAYPOINT_THRESHOLD && wolf.patrolPath != null)
            {
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;
                wolf.patrolPath.AdvanceToNext();
            }
        }

        public void Exit()
        {
            wolf.currentPathNodes = null;
            wolf.currentPathIndex = 0;
            isReturningToStart = false;
            wolf.maxSpeed = wolf.patrolSpeed;
        }
    }

    public class WolvChaseState : IState
    {
        private readonly WolvAgent wolf;
        private const float WAYPOINT_THRESHOLD = 2.5f;

        public WolvChaseState(WolvAgent wolf) => this.wolf = wolf;

        public void Enter()
        {
            wolf.maxSpeed = wolf.chaseSpeed;
            wolf.currentPathIndex = 0;
        }

        public void Update()
        {
       
            if (wolf.targetHunter == null ||
                wolf.targetHunter.IsInSafeZone() ||
                wolf.targetHunter.FSM.CurrentState == wolf.targetHunter.DestrydState ||
                IsPositionInSafeZone(wolf.targetHunter.transform.position) ||
                IsPositionInSafeZone(wolf.lastKnownHunterPosition))
            {
                wolf.targetHunter = null;
                wolf.hasTargetPosition = false;
                wolf.lastKnownHunterPosition = Vector3.zero;
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;
                wolf.FSM.ChangeState(wolf.PatrolState);
                return;
            }

            Vector3 targetPos = wolf.targetHunter.transform.position;
            wolf.lastKnownHunterPosition = targetPos;

            Vector3 flatWolf = new Vector3(wolf.transform.position.x, 0f, wolf.transform.position.z);
            Vector3 flatTarget = new Vector3(targetPos.x, 0f, targetPos.z);
            float distanceToHunter = Vector3.Distance(flatWolf, flatTarget);

            if (distanceToHunter <= wolf.captureRadius)
            {
                wolf.targetHunter.FSM.ChangeState(wolf.targetHunter.DestrydState);
                wolf.targetHunter = null;
                wolf.hasTargetPosition = false;
                wolf.lastKnownHunterPosition = Vector3.zero;
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;
                wolf.FSM.ChangeState(wolf.PatrolState);
                return;
            }

            Vector3 dirToHunter = (targetPos - wolf.transform.position);
            float distToTarget = dirToHunter.magnitude;

            bool hasLineOfSight = !Physics.Raycast(wolf.transform.position + Vector3.up * 0.5f, dirToHunter.normalized, distToTarget, wolf.wallLayer);

            if (hasLineOfSight && !IsPositionInSafeZone(targetPos))
            {
                wolf.currentPathNodes = null;
                wolf.currentPathIndex = 0;
                wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPos));
            }
            else
            {
                if (wolf.currentPathNodes == null || wolf.currentPathNodes.Count == 0)
                {
                    if (GraphManager.Instance != null)
                    {
                        GraphNode startNode = GraphManager.Instance.GetNearestNode(wolf.transform.position);
                        GraphNode targetNode = GraphManager.Instance.GetNearestNode(targetPos);

                        if (targetNode != null && targetNode.isSafeZone)
                        {
                            wolf.targetHunter = null;
                            wolf.hasTargetPosition = false;
                            wolf.lastKnownHunterPosition = Vector3.zero;
                            wolf.currentPathNodes = null;
                            wolf.currentPathIndex = 0;
                            wolf.FSM.ChangeState(wolf.PatrolState);
                            return;
                        }

                        if (startNode != null && targetNode != null)
                        {
                            wolf.currentPathNodes = APathFinderv.FindPath(startNode, targetNode, true);
                            wolf.currentPathIndex = 0;
                        }
                    }
                }

                if (wolf.currentPathNodes != null && wolf.currentPathNodes.Count > 0)
                {
                    if (wolf.currentPathIndex < wolf.currentPathNodes.Count)
                    {
                        Vector3 waypointPos = wolf.currentPathNodes[wolf.currentPathIndex].transform.position;
                        Vector3 flatWaypoint = new Vector3(waypointPos.x, 0f, waypointPos.z);
                        float nodeDist = Vector3.Distance(flatWolf, flatWaypoint);

                        wolf.AddForce(SteeringBehaviors.Seek(wolf, waypointPos));

                        bool reachedNode = nodeDist < WAYPOINT_THRESHOLD;
                        bool passedNode = false;

                        if (!reachedNode && wolf.Velocity.sqrMagnitude > 0.1f)
                        {
                            Vector3 dirToNode = (flatWaypoint - flatWolf).normalized;
                            float dot = Vector3.Dot(wolf.Velocity.normalized, dirToNode);
                            if (dot < -0.3f && nodeDist < WAYPOINT_THRESHOLD * 2f)
                            {
                                passedNode = true;
                            }
                        }

                        if (reachedNode || passedNode)
                        {
                            wolf.currentPathIndex++;
                        }
                    }
                    else
                    {
                        float finalDist = Vector3.Distance(flatWolf, flatTarget);
                        if (finalDist > WAYPOINT_THRESHOLD)
                        {
                            wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPos));
                        }
                        else
                        {
                            wolf.targetHunter = null;
                            wolf.hasTargetPosition = false;
                            wolf.lastKnownHunterPosition = Vector3.zero;
                            wolf.currentPathNodes = null;
                            wolf.currentPathIndex = 0;
                            wolf.FSM.ChangeState(wolf.PatrolState);
                        }
                    }
                }
                else
                {
                    float finalDist = Vector3.Distance(flatWolf, flatTarget);
                    if (finalDist > WAYPOINT_THRESHOLD)
                    {
                        wolf.AddForce(SteeringBehaviors.Seek(wolf, targetPos));
                    }
                    else
                    {
                        wolf.targetHunter = null;
                        wolf.hasTargetPosition = false;
                        wolf.lastKnownHunterPosition = Vector3.zero;
                        wolf.currentPathNodes = null;
                        wolf.currentPathIndex = 0;
                        wolf.FSM.ChangeState(wolf.PatrolState);
                    }
                }
            }
        }

        public void Exit()
        {
            wolf.targetHunter = null;
            wolf.currentPathNodes = null;
            wolf.currentPathIndex = 0;
        }

        private bool IsPositionInSafeZone(Vector3 pos)
        {
            return WolvAgent.IsInsideSafeZone(pos);
        }
    }
}