using UnityEngine;

public class AIZombieStateAlerted1 : AIZombieState
{
    [SerializeField] [Range(1, 60)] float maxDuration = 10.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override AIStateType OnUpdate()
    {
        return AIStateType.Alerted;
    }
}
