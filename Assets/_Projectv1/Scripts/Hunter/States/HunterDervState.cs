using UnityEngine;
using FSM;
using Steering;
using Zones;

namespace Hunter.States
{
    public class HunterDervState : IState
    {
        private readonly HunterNPC hunter;
        private float waitTimer = 0f;
        private float shelterTimer = 0f; 
        private bool hasReachedCenter = false;

        public HunterDervState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter()
        {
            Debug.Log("El HunterNPC,Lobo detectado,Calculando A hacia la SafZonv...");

            waitTimer = 0f;
            shelterTimer = 0f;
            hasReachedCenter = false;

            Transform nrvSaf = hunter.GetNrvSafZonv();

            if (GraphManager.Instance != null && nrvSaf != null)
            {
                GraphNode start = GraphManager.Instance.GetNearestNode(hunter.transform.position);
                GraphNode target = GraphManager.Instance.GetNearestNode(nrvSaf.position);
                hunter.SetPath(APathFinderv.FindPath(start, target, false));
            }
        }

        public void Update()
        {
            Transform nrvSaf = hunter.GetNrvSafZonv();

        
            if (!hasReachedCenter)
            {
                if (hunter.currentPath != null && hunter.currentPathIndex < hunter.currentPath.Count)
                {
                    Vector3 target = hunter.GetCurrentPathTarget();
                    hunter.AddForce(SteeringBehaviors.Seek(hunter, target));

                    if (Vector3.Distance(hunter.transform.position, target) < 2f)
                    {
                        hunter.AdvancePath();
                    }
                }
                else if (nrvSaf != null)
                {
                    hunter.AddForce(SteeringBehaviors.Seek(hunter, nrvSaf.position));
                }

                float distToCenter = nrvSaf != null ? Vector3.Distance(hunter.transform.position, nrvSaf.position) : 999f;
                if (distToCenter < 1.5f || hunter.IsInSafeZone())
                {
                    hasReachedCenter = true;
                    hunter.Stop();
                    Debug.Log("El HunterNPC,Llegó a la SafZonv. Esperando..");
                }
            }
            else
            {
               
                waitTimer += Time.deltaTime;
                shelterTimer += Time.deltaTime;
                hunter.Stop();

               
                bool timeCompleted = waitTimer >= hunter.SafeZoneWaitTime;
                bool safetyTimeout = shelterTimer >= 6.0f;

                if (timeCompleted || safetyTimeout)
                {
                   
                    if (hunter.DetectWolf() && !safetyTimeout)
                    {
                        waitTimer = 0f;
                    }
                    else
                    {
                        if (safetyTimeout && hunter.DetectWolf())
                        {
                            
                        }
                        else
                        {
                            
                        }

                        hunter.SetPath(null);

                        if (hunter.CollectedCount >= hunter.RequiredGathers)
                        {
                            hunter.FSM.ChangeState(hunter.ExtState);
                        }
                        else
                        {
                            hunter.FSM.ChangeState(hunter.SearchState);
                        }
                    }
                }
            }
        }

        public void Exit()
        {
            hunter.SetPath(null);
            waitTimer = 0f;
            shelterTimer = 0f;
            hasReachedCenter = false;
        }
    }
}