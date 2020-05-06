using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private float maxDuration = 40.0f;

    private float timer = 0.0f;
    private float repathTimer = 0.0f;
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
        repathTimer = 0.0f;
        targetReached = false;

        zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.TargetPosition);
        zombieStateMachine.NavAgent.isStopped = false;
    }

    public override AIStateType OnUpdate()
    {
        timer += Time.deltaTime;
        repathTimer += Time.deltaTime;

        if (timer > maxDuration)
            return AIStateType.Patrol;

        if (stateMachine.TargetType == AITargetType.VisualPlayer && zombieStateMachine.InMeleeRange)
            return AIStateType.Attack;

        if (targetReached)
        {
            switch (stateMachine.TargetType)
            {
                case AITargetType.Audio:
                case AITargetType.VisualLight:
                    stateMachine.Clear();
                    return AIStateType.Alerted;

                case AITargetType.VisualFood:
                    return AIStateType.Feeding;
            }
        }

        if (zombieStateMachine.NavAgent.isPathStale ||
            !zombieStateMachine.NavAgent.hasPath ||
            zombieStateMachine.NavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }

        if (!zombieStateMachine.UseRootRotation && zombieStateMachine.TargetType == AITargetType.VisualPlayer
            && zombieStateMachine.visualThreat.Type == AITargetType.VisualPlayer && targetReached)
        {
            Vector3 targetPos = zombieStateMachine.TargetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = newRot;
        }

        else if (!stateMachine.UseRootRotation && !targetReached)
        {
            Quaternion newRot = Quaternion.LookRotation(zombieStateMachine.NavAgent.desiredVelocity);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime);
        }
        else if (targetReached)
        {
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualPlayer)
        {
            // The position is different - maybe same threat, but it has moved, so repath periodically
            if (zombieStateMachine.TargetPosition != zombieStateMachine.visualThreat.Position)
            {
                // Repath more frequently as we get to the target (try and save some CPU cycles)
                if (Mathf.Clamp(zombieStateMachine.visualThreat.Distance * repathDistanceMultiplier, repathVisualMinDuration, repathVisualMaxDuration) < repathTimer)
                {
                    zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.visualThreat.Position);
                    repathTimer = 0.0f;
                }
            }

            stateMachine.SetTarget(zombieStateMachine.visualThreat);
        }

        return AIStateType.Pursuit;
    }
}
