using UnityEngine;

public class AIZombieStateIdle1 : AIZombieState
{
    [SerializeField] Vector2 idleTimeRange = new Vector2(10.0f, 60.0f);

    private float idleTime = 0.0f;
    private float timer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

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
        zombieStateMachine.Clear();
    }

    public override AIStateType OnUpdate()
    {

        return AIStateType.Idle;
    }
}
