using UnityEngine;

public class AIZombieStateAlerted1 : AIZombieState
{
    [SerializeField] [Range(1, 60)] float maxDuration = 10.0f;
    [SerializeField] float waypointAngleThreshold = 90.0f;
    [SerializeField] float threatAngleThreshold = 10.0f;


    private float timer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Alerted state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Alerted State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        // Configure state machine
        zombieStateMachine.NavAgentControl(true, false);
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = false;
        zombieStateMachine.AttackType = 0;

        timer = maxDuration;
    }

    public override AIStateType OnUpdate()
    {
        timer -= Time.deltaTime;

        if (timer <= 0.0f) return AIStateType.Alerted;

        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualPlayer)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Pursuit;
        }
        if (zombieStateMachine.audioThreat.Type == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            timer = maxDuration;
        }
        if (zombieStateMachine.visualThreat.Type == AITargetType.VisualLight)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            timer = maxDuration;
        }
        if (zombieStateMachine.audioThreat.Type == AITargetType.None
            && zombieStateMachine.visualThreat.Type == AITargetType.VisualFood)
        {
            zombieStateMachine.SetTarget(stateMachine.visualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if (zombieStateMachine.TargetType == AITargetType.Audio || zombieStateMachine.TargetType == AITargetType.VisualLight)
        {
            angle = FindSignedAngle(zombieStateMachine.transform.forward, zombieStateMachine.TargetPosition - zombieStateMachine.transform.position);

            if (zombieStateMachine.TargetType == AITargetType.Audio && Mathf.Abs(angle) < threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if (Random.value < zombieStateMachine.Intelligence)
            {
                zombieStateMachine.Seeking = (int)Mathf.Sign(angle);
            }
            else
                zombieStateMachine.Seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
        }
        else if (zombieStateMachine.TargetType == AITargetType.Waypoint)
        {
            angle = FindSignedAngle(zombieStateMachine.transform.forward, zombieStateMachine.NavAgent.steeringTarget - zombieStateMachine.transform.position);

            if (Mathf.Abs(angle) < waypointAngleThreshold)
                return AIStateType.Patrol;
            zombieStateMachine.Seeking = (int)Mathf.Sign(angle);
        }
        return AIStateType.Alerted;
    }
}
