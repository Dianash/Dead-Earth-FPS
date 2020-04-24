using UnityEngine;

public class AIStateMachineLink : StateMachineBehaviour
{
    protected AIStateMachine stateMachine;
    public AIStateMachine StateMachine
    {
        set
        {
            stateMachine = value;
        }
    }
}
