using UnityEngine;

public class AIZombieStateFeeding : AIZombieState
{
    [SerializeField] float slerpSpeed = 5.0f;

    private int eatingHash = Animator.StringToHash("Feeding State");
    private int eatingLayerIndex = -1;

    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Feeding state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Feeding State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        if (eatingLayerIndex == -1)
            eatingLayerIndex = zombieStateMachine.Animator.GetLayerIndex("Cinematic Layer");

        // Configure State Machine`s Animator
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = true;
        zombieStateMachine.AttackType = 0;

        zombieStateMachine.NavAgentControl(true, false);
    }   

    public override void OnExitState()
    {
        if (zombieStateMachine != null)
            zombieStateMachine.Feeding = false;
    }
}
