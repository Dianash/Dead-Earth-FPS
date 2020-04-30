using UnityEngine;

public class AIZombieStateIdle1 : AIZombieState
{
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    public override AIStateType OnUpdate()
    {

        return AIStateType.Idle;
    }
}
