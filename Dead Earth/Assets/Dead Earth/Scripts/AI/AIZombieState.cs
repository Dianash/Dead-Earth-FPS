using UnityEngine;

public abstract class AIZombieState : AIState
{
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (stateMachine == null)
            return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType currentType = stateMachine.visualThreat.Type;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(stateMachine.SensorPosition, other.transform.position);

                if (currentType != AITargetType.VisualPlayer || (currentType == AITargetType.VisualPlayer &&
                    distance < stateMachine.visualThreat.Distance))
                {

                }
            }
        }
    }
}
