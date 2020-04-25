using UnityEngine;

public abstract class AIZombieState : AIState
{
    protected int playerLayerMask = -1;
    protected int bodyPartLayer = -1;

    private void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        bodyPartLayer = LayerMask.GetMask("AI Body Part");
    }

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
                    if (ColliderIsVisible(other, out RaycastHit hitInfo, playerLayerMask))
                    {
                        stateMachine.visualThreat.Set(AITargetType.VisualPlayer, other, other.transform.position, distance);
                    }
                }
            }
        }
    }

    private bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();

        if (stateMachine == null || stateMachine.GetType() != typeof(AIZombieStateMachine))
            return false;

        AIZombieStateMachine zombieMachine = (AIZombieStateMachine)stateMachine;

        Vector3 head = stateMachine.SensorPosition;
        Vector3 direction = other.transform.position - head;

        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > zombieMachine.Fov * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, stateMachine.SensorRadius * zombieMachine.Sight, layerMask);

        // Find the closest collider that is not the AIs own body part. If its not the target, than the target is obstructed.
        float closestColliderDistance = float.MaxValue;
        Collider closestCollider = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.distance < closestColliderDistance)
            {
                if (hit.transform.gameObject.layer == bodyPartLayer)
                {
                    if (stateMachine != GameSceneManager.Instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    closestColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        if (closestCollider && closestCollider.gameObject == other.gameObject)
            return true;

        return false;
    }
}
