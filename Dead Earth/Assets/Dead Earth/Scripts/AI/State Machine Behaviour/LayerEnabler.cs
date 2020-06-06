using UnityEngine;

public class LayerEnabler : AIStateMachineLink
{
    public bool onEnter = false;
    public bool onExit = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), onEnter);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (stateMachine)
            stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), onExit);
    }
}
