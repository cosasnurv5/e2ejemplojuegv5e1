using UnityEngine;
using FSM;
using Environment;

namespace Boids.States
{
    public class BoidIntrState : IState
    {
        private readonly BoidAgent boid;

        public BoidIntrState(BoidAgent boid) => this.boid = boid;

        public void Enter() { }

        public void Update()
        {
            if (boid.HitsReceived >= 2)
            {
                boid.FSM.ChangeState(boid.DestroydState);
                return;
            }

            if (boid.Sensor.GetNearestHunter(boid.visionRadius) != null)
            {
                boid.FSM.ChangeState(boid.EvadState);
                return;
            }

            if (boid.CurrentInterestTarget == null)
            {
                boid.FSM.ChangeState(boid.FlockngState);
                return;
            }

            if (boid.CurrentInterestTarget.TryGetComponent<InterestObject>(out var targetObj))
            {
                targetObj.TakeDamage(boid.damagePerSecond * Time.deltaTime);
            }
            else
            {
                boid.CurrentInterestTarget = null;
                boid.FSM.ChangeState(boid.FlockngState);
            }
        }

        public void Exit() { }
    }
}