public class StateMachine
{
    // SIRVE PARA MANTENER TRACKEADO (SEGUIMIENTO) DEL ESTADO DEL JUGADOR
    
    public State currentState;

    public void Initialize(State startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(State newState)
    {
        currentState.Exit();

        currentState = newState;
        newState.Enter();
    }
}
