using UnityEngine;

public class AIStateMachineLink : StateMachineBehaviour
{
    protected AIStateMachine stateMachine;

    public AIStateMachine StateMachine { get => stateMachine; set => stateMachine = value; }
}
