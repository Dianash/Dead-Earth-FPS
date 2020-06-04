using UnityEngine;

/// <summary>
/// Base class of zombie states. Provides the event processing and storage of the current threats
/// </summary>
public abstract class AIZombieState : AIState
{
    protected int playerLayerMask = -1;
    protected int bodyPartLayer = -1;
    protected int visualLayerMask = -1;
    protected AIZombieStateMachine zombieStateMachine = null;

    /// <summary>
    /// Calculate the masks and layers used for raycasting and layer testing
    /// </summary>
    private void Awake()
    {
        playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") + 1;

        bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if (stateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            base.SetStateMachine(stateMachine);
            zombieStateMachine = (AIZombieStateMachine)stateMachine;
        }
    }

    /// <summary>
    /// Called by the parent state machine threats enter/stay/exit the zombie`s sensor trigger.
    /// Examines the threat and stores it in the parent machine Visual or Audio threat members,
    /// if found to be the a higher priority threat
    /// </summary>
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        if (zombieStateMachine == null)
            return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType currentType = zombieStateMachine.visualThreat.Type;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(zombieStateMachine.SensorPosition, other.transform.position);

                if (currentType != AITargetType.VisualPlayer || (currentType == AITargetType.VisualPlayer &&
                    (distance < zombieStateMachine.visualThreat.Distance)))
                {
                    if (ColliderIsVisible(other, out RaycastHit hitInfo, playerLayerMask))
                    {
                        zombieStateMachine.visualThreat.Set(AITargetType.VisualPlayer, other, other.transform.position, distance);
                    }
                }
            }
            else if (other.CompareTag("Flash Light") && currentType != AITargetType.VisualPlayer)
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;

                float distanceToThreat = Vector3.Distance(zombieStateMachine.SensorPosition, flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggravationFactor = distanceToThreat / zSize;

                if (aggravationFactor <= zombieStateMachine.Sight && aggravationFactor <= zombieStateMachine.Intelligence)
                {
                    zombieStateMachine.visualThreat.Set(AITargetType.VisualLight, other, other.transform.position, distanceToThreat);
                }
            }
            else if (other.CompareTag("AI Sound Emitter"))
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) return;

                Vector3 agentSensorPosition = zombieStateMachine.SensorPosition;

                Vector3 soundPos;
                float soundRadius;

                ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

                float distanceToThreat = (soundPos - agentSensorPosition).magnitude;
                float distanceFactor = distanceToThreat / soundRadius;

                // Bias the factor based on hearing ability of Agent
                distanceFactor += distanceToThreat * (1.0f - zombieStateMachine.Hearing);

                if (distanceFactor > 1.0f) return;

                if (distanceToThreat < zombieStateMachine.audioThreat.Distance)
                {
                    zombieStateMachine.audioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                }
            }
            else if (other.CompareTag("AI Food") && currentType != AITargetType.VisualPlayer && currentType != AITargetType.VisualLight
                    && zombieStateMachine.Satisfaction <= 0.9f && zombieStateMachine.audioThreat.Type == AITargetType.None)
            {
                float distanceToThreat = Vector3.Distance(other.transform.position, zombieStateMachine.SensorPosition);

                if (distanceToThreat < zombieStateMachine.visualThreat.Distance)
                {
                    RaycastHit hitInfo;

                    if (ColliderIsVisible(other, out hitInfo, visualLayerMask))
                    {
                        zombieStateMachine.visualThreat.Set(AITargetType.VisualFood, other, other.transform.position, distanceToThreat);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Test the passed collider against the zombie`s FOV
    /// </summary>
    private bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();

        if (zombieStateMachine == null || zombieStateMachine.GetType() != typeof(AIZombieStateMachine))
            return false;

        // Calculate the angle between the sensor origin and the direction of the collider
        Vector3 head = zombieStateMachine.SensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(direction, transform.forward);

        // If the angle is greater then half of FOV, then it is outside of view cone
        if (angle > zombieStateMachine.Fov * 0.5f)
            return false;

        // Returns the hits in between the lines of sight
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, zombieStateMachine.SensorRadius * zombieStateMachine.Sight, layerMask);

        // Find the closest collider that is not the AIs own body part. If its not the target, than the target is obstructed
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
