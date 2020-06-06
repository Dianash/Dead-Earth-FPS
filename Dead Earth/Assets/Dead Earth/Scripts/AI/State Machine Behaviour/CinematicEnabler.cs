using UnityEngine;

public class CinematicEnabler : AIStateMachineLink
{
    public bool onEnter = false;
    public bool onExit = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StateMachine)
            StateMachine.CinematicEnabled = onEnter;      

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StateMachine)
            StateMachine.CinematicEnabled = onExit;
    }
}
