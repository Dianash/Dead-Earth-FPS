using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Generic patrolling behavior.
/// </summary>
public class AIZombieStatePatrol1 : AIZombieState
{
    [SerializeField] AIWaypointNetwork waypointNetwork = null;
    [SerializeField] bool randomPatrol = false;
    [SerializeField] int currentWaypoint = 0;
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
        zombieStateMachine.Speed = speed;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = 0;

        if (zombieStateMachine.TargetType != AITargetType.Waypoint)
        {
            zombieStateMachine.Clear();

            if (waypointNetwork != null && waypointNetwork.waypoints.Count > 0)
            {
                if (randomPatrol)
                {
                    currentWaypoint = Random.Range(0, waypointNetwork.waypoints.Count);
                }

                if (currentWaypoint < waypointNetwork.waypoints.Count)
                {
                    Transform waypoint = waypointNetwork.waypoints[currentWaypoint];

                    if (waypoint != null)
                    {
                        zombieStateMachine.SetTarget(AITargetType.Waypoint,
                                                     null,
                                                     waypoint.position,
                                                     Vector3.Distance(zombieStateMachine.transform.position, waypoint.position));

                        zombieStateMachine.NavAgent.SetDestination(waypoint.position);
                    }
                }
            }
        }

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

        float angle = Vector3.Angle(zombieStateMachine.transform.forward,
            zombieStateMachine.NavAgent.steeringTarget - zombieStateMachine.transform.position);

        if (angle > turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if (!zombieStateMachine.UseRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(zombieStateMachine.NavAgent.desiredVelocity);

            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation,
                                                                     newRot,
                                                                     Time.deltaTime * slerpSpeed);
        }

        if (zombieStateMachine.NavAgent.isPathStale || !zombieStateMachine.NavAgent.hasPath
            || zombieStateMachine.NavAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            NextWaipoint();
        }

        return AIStateType.Patrol;
    }   
}
