using UnityEngine;
using FSM;

namespace Boids.States
{
    public class BoidRespwnState : IState
    {
        private readonly BoidAgent boid;

        public BoidRespwnState(BoidAgent boid) => this.boid = boid;

        public void Enter()
        {
            boid.gameObject.SetActive(false);
        }

        public void Update() { }

        public void Exit()
        {
            boid.gameObject.SetActive(true);
        }
    }
}