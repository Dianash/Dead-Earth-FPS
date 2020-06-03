using UnityEngine;

public class AIZombieStateFeeding : AIZombieState
{
    [SerializeField] float slerpSpeed = 5.0f;
    [SerializeField] Transform bloodParticlesMount = null;
    [SerializeField] [Range(0.01f, 1.0f)] float bloodParticlesBurstTime = 0.1f;
    [SerializeField] [Range(1, 100)] int bloodParticlesBurstAmount = 10;

    private int eatingStateHash = Animator.StringToHash("Feeding State");
    private int crawlEatingStateHash = Animator.StringToHash("Crawl Feeding State");
    private int eatingLayerIndex = -1;
    private float timer = 0.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    /// <summary>
    /// Called by the State Machine, when first transitioned into Feeding state
    /// </summary>
    public override void OnEnterState()
    {
        Debug.Log("Entering Feeding State");

        base.OnEnterState();
        if (zombieStateMachine == null) return;

        if (eatingLayerIndex == -1)
            eatingLayerIndex = zombieStateMachine.Animator.GetLayerIndex("Cinematic Layer");

        timer = 0.0f;

        // Configure State Machine`s Animator
        zombieStateMachine.Speed = 0;
        zombieStateMachine.Seeking = 0;
        zombieStateMachine.Feeding = true;
        zombieStateMachine.AttackType = 0;

        zombieStateMachine.NavAgentControl(true, false);
    }

    public override AIStateType OnUpdate()
    {
        timer += Time.deltaTime;
        if (zombieStateMachine.Satisfaction > 0.9f)
        {
            zombieStateMachine.GetWaypointPosition(false);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.visualThreat.Type != AITargetType.None && zombieStateMachine.visualThreat.Type != AITargetType.VisualFood)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.visualThreat);
            return AIStateType.Alerted;
        }

        if (zombieStateMachine.audioThreat.Type == AITargetType.Audio)
        {
            zombieStateMachine.SetTarget(zombieStateMachine.audioThreat);
            return AIStateType.Alerted;
        }

        int currentHash = zombieStateMachine.Animator.GetCurrentAnimatorStateInfo(eatingLayerIndex).shortNameHash;

        if (currentHash == eatingStateHash || currentHash == crawlEatingStateHash)
        {
            zombieStateMachine.Satisfaction = Mathf.Min(zombieStateMachine.Satisfaction + (Time.deltaTime * zombieStateMachine.ReplenishRate) / 100.0f, 1.0f);

            if (GameSceneManager.Instance && GameSceneManager.Instance.BloodParticles && bloodParticlesMount)
            {
                if (timer > bloodParticlesBurstTime)
                {
                    ParticleSystem system = GameSceneManager.Instance.BloodParticles;
                    system.transform.position = bloodParticlesMount.transform.position;
                    system.transform.rotation = bloodParticlesMount.transform.rotation;
                    ParticleSystemSimulationSpace spaceMode = system.main.simulationSpace;
                    spaceMode = ParticleSystemSimulationSpace.World;

                    system.Emit(bloodParticlesBurstAmount);
                    timer = 0.0f;
                }
            }
        }

        if (!zombieStateMachine.UseRootRotation)
        {
            Vector3 targetPos = zombieStateMachine.TargetPosition;
            targetPos.y = zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - zombieStateMachine.transform.position);
            zombieStateMachine.transform.rotation = Quaternion.Slerp(zombieStateMachine.transform.rotation, newRot, Time.deltaTime * slerpSpeed);
        }

        Vector3 headToTarget = zombieStateMachine.TargetPosition - zombieStateMachine.Animator.GetBoneTransform(HumanBodyBones.Head).position;
        zombieStateMachine.transform.position = Vector3.Lerp(zombieStateMachine.transform.position, zombieStateMachine.transform.position + headToTarget, Time.deltaTime);

        return AIStateType.Feeding;
    }

    public override void OnExitState()
    {
        if (zombieStateMachine != null)
            zombieStateMachine.Feeding = false;
    }
}
