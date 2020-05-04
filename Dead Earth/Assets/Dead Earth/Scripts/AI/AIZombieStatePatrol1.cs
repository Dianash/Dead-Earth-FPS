using UnityEngine;

/// <summary>
/// Generic patrolling behavior.
/// </summary>
public class AIZombieStatePatrol1 : AIZombieState
{
    [SerializeField] AIWaypointNetwork waypointNetwork = null;
    [SerializeField] bool randomPatrol = false;
    [SerializeField] int currentWaypoint = 0;
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
                    currentWaypoint = Random.Range(0, waypointNetwork.waypoints.Count - 1);
                }

                if (currentWaypoint < waypointNetwork.waypoints.Count)
                {
                    Transform waypoint = waypointNetwork.waypoints[currentWaypoint];

                    if (waypoint != null)
                    {
                        zombieStateMachine.SetTarget(AITargetType.Waypoint, null, waypoint.position,
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
        return AIStateType.Patrol;
    }
}
