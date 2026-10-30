using UnityEngine;
using FSM;
using Wolves;

namespace Hunter.States
{
    public class HunterDestrydState : IState
    {
        private readonly HunterNPC hunter;
        private float respawnTimer = 0f;

        public HunterDestrydState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter()
        {
            Debug.Log("El HunterNPC,alcanzado por un lobo.");
            respawnTimer = 0f;
            hunter.CollectedCount = 0;
            hunter.Stop();

            ToggleVisuals(false);
            hunter.TeleportToInitialZone();
        }

        public void Update()
        {
            respawnTimer += Time.deltaTime;

            
            if (respawnTimer >= hunter.RespawnTime)
            {
                Debug.Log("El HunterNPC,Reseteo...");

                ToggleVisuals(true);
                hunter.TeleportToInitialZone();
                hunter.FSM.ChangeState(hunter.SearchState);
            }
        }

        public void Exit()
        {
            respawnTimer = 0f;
            ToggleVisuals(true);

         
          
            foreach (var wolf in WolvAgent.allWolves)
            {
                if (wolf != null && wolf.FSM != null)
                {
                  
                    wolf.FSM.ChangeState(wolf.PatrolState);
                }
            }
        }

        private void ToggleVisuals(bool isVisible)
        {
            foreach (var renderer in hunter.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = isVisible;
            }
            foreach (var col in hunter.GetComponentsInChildren<Collider>())
            {
                col.enabled = isVisible;
            }
        }
    }
}