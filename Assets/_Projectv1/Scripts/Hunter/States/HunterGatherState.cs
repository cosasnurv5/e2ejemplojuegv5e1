using UnityEngine;
using FSM;
using Steering;
using Managers;

namespace Hunter.States
{
    public class HunterGatherState : IState
    {
        private readonly HunterNPC hunter;
        private float gatherTimer = 0f;
        private const float GATHER_DURATION = 2f;
        private const float ARRIVE_RADIUS_SQR = 4.0f;

        public HunterGatherState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter() => gatherTimer = 0f;

        public void Update()
        {
            var target = hunter.DestroyedTarget;

            if (target == null || !target.IsDestroyed)
            {
                hunter.DestroyedTarget = null;
                hunter.FSM.ChangeState(hunter.SearchState);
                return;
            }

            float sqrDistance = (hunter.transform.position - target.transform.position).sqrMagnitude;

            if (sqrDistance > ARRIVE_RADIUS_SQR)
            {
                hunter.AddForce(SteeringBehaviors.Arrive(hunter, target.transform.position, 2f));
                gatherTimer = 0f;
            }
            else
            {
                hunter.Stop();
                gatherTimer += Time.deltaTime;

                if (gatherTimer >= GATHER_DURATION)
                {
                    GameManager.Instance.CollectAndRespawnBoid(target);
                    hunter.DestroyedTarget = null;
                    hunter.CollectedCount++;

                   
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

        public void Exit() { }
    }
}