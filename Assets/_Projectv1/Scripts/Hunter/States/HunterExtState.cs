using UnityEngine;
using FSM;
using Steering;
using Zones;

namespace Hunter.States
{
    public class HunterExtState : IState
    {
        private readonly HunterNPC hunter;

        public HunterExtState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter()
        {
            Debug.Log("El HunterNPC,en modo Exit. Recolecciones: " + hunter.CollectedCount);

            if (GraphManager.Instance != null && ExtZonv.Instance != null)
            {
                GraphNode start = GraphManager.Instance.GetNearestNode(hunter.transform.position);
                GraphNode target = GraphManager.Instance.GetNearestNode(ExtZonv.Instance.transform.position);
                hunter.SetPath(APathFinderv.FindPath(start, target, false));
            }
        }

        public void Update()
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
            else if (ExtZonv.Instance != null)
            {
                hunter.AddForce(SteeringBehaviors.Seek(hunter, ExtZonv.Instance.transform.position));
            }

           
            float distToExit = ExtZonv.Instance != null ? Vector3.Distance(hunter.transform.position, ExtZonv.Instance.transform.position) : 999f;
            bool isInsideExit = hunter.IsAtExitZone() || distToExit < 3.0f;

            
        
           

          
            if (isInsideExit && hunter.CollectedCount >= hunter.RequiredGathers)
            {
                Debug.Log("El HunterNPC,condiciones para el modo exit! Reseteando y teletransportando...");

               
                hunter.CollectedCount = 0;

               
                hunter.TeleportToInitialZone();

            
                hunter.FSM.ChangeState(hunter.SearchState);
            }
        }

        public void Exit()
        {
            hunter.SetPath(null);
        }
    }
}