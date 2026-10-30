using UnityEngine;
using FSM;

namespace Hunter.States
{
    public class HunterRvState : IState
    {
        private readonly HunterNPC hunter;

        public HunterRvState(HunterNPC hunter) => this.hunter = hunter;

        public void Enter() => hunter.Stop();

        public void Update()
        {
            hunter.Energy += hunter.energyR * Time.deltaTime;
            if (hunter.Energy >= hunter.maxEnergy)
            {
                hunter.Energy = hunter.maxEnergy;
                hunter.FSM.ChangeState(hunter.SearchState);
            }
        }

        public void Exit() { }
    }
}