using UnityEngine;

/// <summary>
/// An AIState that implements a zombie`s Idle behavior
/// </summary>
public class AIZombieStateIdle1 : AIZombieState
{
    [SerializeField] Vector2 idleTimeRange = new Vector2(10.0f, 60.0f);

    private float idleTime = 0.0f;
    private float timer = 0.0f;

    /// <returns>
    /// Returns the type of the state
    /// </returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Idle state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Idle State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        // Set Idle Time
        idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        timer = 0.0f;

        // Configure state machine
        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = 0;
        zombieStateMachine.ClearTarget();
    }

    public override AIStateType OnUpdate()
    {
        if (zombieStateMachine == null) return AIStateType.Idle;

        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualPlayer)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }
        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualLight)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }
        if (zombieStateMachine.audioThreat.Type == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }
        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualFood)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        timer += Time.deltaTime;

        if (timer > idleTime)
        {
            zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.GetWaypointPosition(false));
            zombieStateMachine.NavAgent.isStopped = false;
            return AIStateType.Patrol;
        }

        return AIStateType.Idle;
    }
}
