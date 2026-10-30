using UnityEngine;
using FSM;

namespace Boids.States
{
    public class BoidDestroydState : IState
    {
        private readonly BoidAgent boid;

        public BoidDestroydState(BoidAgent boid) => this.boid = boid;

        public void Enter()
        {
            boid.IsDestroyed = true;
            boid.StopAgent();
        }

        public void Update() { }

        public void Exit() { }
    }
}