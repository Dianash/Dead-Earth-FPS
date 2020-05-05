using UnityEngine;

/// <summary>
/// A Zombie state used for pursuing a target
/// </summary>
public class AIZombieStatePursuit1 : AIZombieState
{
    [SerializeField] [Range(0, 10)] private float speed = 1.0f;
    [SerializeField] private float slerpSpeed = 5.0f;
    [SerializeField] private float repathDistanceMultiplier = 0.035f;
    [SerializeField] private float repathVisualMinDuration = 0.05f;
    [SerializeField] private float repathVisualMaxDuration = 5.0f;
    [SerializeField] private float repathAudioMinDuration = 0.25f;
    [SerializeField] private float repathAudioMaxDuration = 5.0f;

    private float timer = 0.0f;
    private bool targetReached = false;

    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Pursuit state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Pursuit State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;        

        // Configure state machine
        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = 0;

        timer = 0.0f;
        targetReached = false;

        zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.TargetPosition);
        zombieStateMachine.NavAgent.isStopped = false;
    }

    public override AIStateType OnUpdate()
    {


        return AIStateType.Pursuit;
    }
}
