using UnityEngine;
using FSM;

namespace Boids.States
{
    public class BoidFlockngState : IState
    {
        private readonly BoidAgent boid;

        public BoidFlockngState(BoidAgent boid) => this.boid = boid;

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

            Transform interestObj = boid.Sensor.GetNearestInterestObject(boid.visionRadius);
            if (interestObj != null)
            {
                boid.CurrentInterestTarget = interestObj;
                boid.FSM.ChangeState(boid.SkObjState);
                return;
            }

            
            boid.ApplyFlockingForces();
        }

        public void Exit() { }
    }
}