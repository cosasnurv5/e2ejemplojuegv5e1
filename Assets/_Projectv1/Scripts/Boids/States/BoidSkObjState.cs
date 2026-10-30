using UnityEngine;
using FSM;
using Steering;

namespace Boids.States
{
    public class BoidSkObjState : IState
    {
        private readonly BoidAgent boid;

        public BoidSkObjState(BoidAgent boid) => this.boid = boid;

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

            float dist = Vector3.Distance(boid.transform.position, boid.CurrentInterestTarget.position);
            if (dist <= boid.interactDistance)
            {
                boid.FSM.ChangeState(boid.IntrState); 
                return;
            }

            Vector3 arriveForce = SteeringBehaviors.Arrive(boid, boid.CurrentInterestTarget.position, 2f) * boid.arriveWeight;
            boid.AddForce(arriveForce);
        }

        public void Exit() { }
    }
}