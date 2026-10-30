using UnityEngine;
using FSM;
using Steering;

namespace Hunter.States
{
    public class HunterAttackState : IState
    {
        private readonly HunterNPC hunter;

        public HunterAttackState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter() { }

        public void Update()
        {
          
            var destroyedBoid = hunter.FindDestroyedBoidInVision();
            if (destroyedBoid != null)
            {
                hunter.DestroyedTarget = destroyedBoid;
                hunter.FSM.ChangeState(hunter.GatherState);
                return;
            }

            var target = hunter.CurrentTarget;

          
            if (target == null || target.IsDestroyed)
            {
                var newTarget = hunter.FindValidBoidInVision();
                if (newTarget != null)
                {
                    hunter.CurrentTarget = newTarget;
                    target = newTarget;
                }
                else
                {
                    hunter.FSM.ChangeState(hunter.SearchState);
                    return;
                }
            }

            float distance = Vector3.Distance(hunter.transform.position, target.transform.position);

       
            hunter.AddForce(SteeringBehaviors.Pursuit(hunter, target));

          
            if (distance <= hunter.MeleeAttackRadius)
            {
                if (hunter.TBATimer <= 0f) ExeAttack(target, 2);
            }
            else if (distance <= hunter.RangeAttackRadius)
            {
                if (hunter.TBATimer <= 0f) ExeAttack(target, 1);
            }
        }

        private void ExeAttack(Boids.BoidAgent target, int hits)
        {
            target.ReceiveHit(hits);
            hunter.TBATimer = hunter.TBA;

            if (target.IsDestroyed)
            {
                hunter.DestroyedTarget = target;
                hunter.CurrentTarget = null;
                hunter.FSM.ChangeState(hunter.GatherState);
            }
            else
            {
                hunter.FSM.ChangeState(hunter.SearchState);
            }
        }

        public void Exit() { }
    }
}