namespace FSM
{
    public class FiniteStateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            if (newState == null || CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        } 
          
        public void Update()
        {
            CurrentState?.Update();
        }
    }
}