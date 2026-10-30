using UnityEngine;
using FSM;
using Steering;

namespace Boids.States
{
    public class BoidEvadState : IState
    {
        private readonly BoidAgent boid;

        public BoidEvadState(BoidAgent boid) => this.boid = boid;

        public void Enter() { }

        public void Update()
        {
            if (boid.HitsReceived >= 2)
            {
                boid.FSM.ChangeState(boid.DestroydState);
                return;
            }

            Transform hunter = boid.Sensor.GetNearestHunter(boid.visionRadius);
            if (hunter == null)
            {
                boid.FSM.ChangeState(boid.FlockngState);
                return;
            }

            if (hunter.TryGetComponent<SteeringAgent>(out var hunterAgent))
            {
                Vector3 evadeForce = SteeringBehaviors.Evade(boid, hunterAgent) * boid.evadeWeight;
                boid.AddForce(evadeForce);

                
                var neighbors = boid.Sensor.GetNeighbors(boid.separationRadius);
                boid.AddForce(boid.CalculateSeparation(neighbors) * boid.separationWeight);
            }
        }

        public void Exit() { }
    }
}