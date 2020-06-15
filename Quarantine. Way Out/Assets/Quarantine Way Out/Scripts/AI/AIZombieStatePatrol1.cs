using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Generic patrolling behavior.
/// </summary>
public class AIZombieStatePatrol1 : AIZombieState
{
    [SerializeField] float turnOnSpotThreshold = 80.0f;
    [SerializeField] float slerpSpeed = 5.0f;
    [SerializeField] [Range(0.0f, 3.0f)] float speed = 1.0f;

    /// <summary>
    /// Called by parent State Machine to get this states type. 
    /// </summary>
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Idle state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering patrol State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        // Configure state machine
        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = 0;

        zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.GetWaypointPosition(false));

        zombieStateMachine.NavAgent.isStopped = false;
    }

    /// <summary>
    /// Called by the state machine each frame to give this state a time-slice to update itself.
    /// </summary>
    public override AIStateType OnUpdate()
    {
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

        // Dead body case
        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualFood)
        {
            if ((1.0f - zombieStateMachine.Satisfaction) > (zombieStateMachine.visualThreat.Distance / zombieStateMachine.SensorRadius))
            {
                zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
                return AIStateType.Pursuit;
            }
        }
        if (zombieStateMachine.NavAgent.pathPending)
        {
            zombieStateMachine.Speed = 0;
            return AIStateType.Patrol;
        }
        else
        {
            zombieStateMachine.Speed = speed;
        }

        float angle = Vector3.Angle(zombieStateMachine.transform.forward,
            zombieStateMachine.NavAgent.steeringTarget - zombieStateMachine.transform.position);

        if (angle > turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if (!zombieStateMachine.UseRootRotation)
        {
            if (zombieStateMachine.NavAgent.desiredVelocity != Vector3.zero)
            {
                Quaternion newRot = Quaternion.LookRotation(zombieStateMachine.NavAgent.desiredVelocity);

                zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
            }
        }

        if (zombieStateMachine.NavAgent.isPathStale || zombieStateMachine.NavAgent.hasPath
            || zombieStateMachine.NavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            zombieStateMachine.GetWaypointPosition(true);
        }

        return AIStateType.Patrol;
    }

    public override void OnDestinationtReached(bool isReached)
    {
        if (zombieStateMachine == null || !isReached)
            return;

        if (zombieStateMachine.TargetType == AITargetType.Waypoint)
            zombieStateMachine.GetWaypointPosition(true);
    }
}
