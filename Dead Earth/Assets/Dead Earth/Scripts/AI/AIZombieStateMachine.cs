using UnityEngine;

public class AIZombieStateMachine : AIStateMachine
{
    #region Serialized fields

    [SerializeField] [Range(10.0f, 360.0f)] float fov = 50.0f;

    [SerializeField] [Range(0.0f, 1.0f)] float sight = 0.5f;

    [SerializeField] [Range(0.0f, 1.0f)] float hearing = 50.0f;

    [SerializeField] [Range(0.0f, 1.0f)] float aggression = 0.5f;

    [SerializeField] [Range(0, 100)] int health = 100;

    [SerializeField] [Range(0, 100)] int lowerBodyDamage = 0;

    [SerializeField] [Range(0, 100)] int upperBodyDamage = 0;

    [SerializeField] [Range(0, 100)] int upperBodyThreshold = 30;

    [SerializeField] [Range(0, 100)] int limpThreshold = 30;

    [SerializeField] [Range(0, 100)] int crawlThreshold = 90;

    [SerializeField] [Range(0.0f, 1.0f)] float intelligence = 0.5f;

    [SerializeField] [Range(0.0f, 1.0f)] float satisfaction = 1f;

    [SerializeField] float replenishRate = 0.5f;

    [SerializeField] float depletionRate = 0.1f;

    #endregion

    #region Hashes

    private int speedHash = Animator.StringToHash("Speed");
    private int seekingHash = Animator.StringToHash("Seeking");
    private int feedingHash = Animator.StringToHash("Feeding");
    private int attackHash = Animator.StringToHash("Attack");
    private int crawlHash = Animator.StringToHash("Crawling");

    private AIBoneControlType boneControlType = AIBoneControlType.Animated;

    #endregion

    #region Public properties

    public float Fov { get => fov; }

    public float Hearing { get => hearing; }

    public float Sight { get => sight; }

    public float Intelligence { get => intelligence; }

    public bool Crawling { get; } = false;

    public bool Feeding { get; set; } = false;

    public int AttackType { get; set; } = 0;

    public int Seeking { get; set; } = 0;

    public float Speed { get; set; } = 0.0f;

    public float ReplenishRate { get => replenishRate; }

    public float Satisfaction
    {
        get => satisfaction;
        set => satisfaction = value;
    }

    public float Aggression
    {
        get => aggression;
        set => aggression = value;
    }

    public int Health
    {
        get => health;
        set => health = value;
    }

    public bool IsCrawling
    {
        get { return lowerBodyDamage >= crawlThreshold; }
    }

    #endregion

    protected override void Start()
    {
        base.Start();
        UpdateAnimatorDamage();
    }

    /// <summary>
    /// Refreshes the animator with up-to-date values for its parameters 
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (animator != null)
        {
            animator.SetFloat(speedHash, Speed);
            animator.SetBool(feedingHash, Feeding);
            animator.SetInteger(seekingHash, Seeking);
            animator.SetInteger(attackHash, AttackType);
        }

        satisfaction = Mathf.Max(0, satisfaction - (depletionRate * Time.deltaTime / 100.0f) * Mathf.Pow(Speed, 3));
    }

    protected void UpdateAnimatorDamage()
    {
        if (animator != null)
        {
            animator.SetBool(crawlHash, IsCrawling);
        }
    }

    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager character, int hitDirection)
    {
        if (GameSceneManager.Instance != null)
        {
            if (GameSceneManager.Instance.BloodParticles != null)
            {
                ParticleSystem particleSystem = GameSceneManager.Instance.BloodParticles;
                particleSystem.transform.position = position;
                ParticleSystemSimulationSpace spaceMode = particleSystem.main.simulationSpace;
                spaceMode = ParticleSystemSimulationSpace.World;
                particleSystem.Emit(60);
            }
        }

        float hitStrength = force.magnitude;

        if (boneControlType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (hitStrength > 1.0f)
                {
                    bodyPart.AddForce(force, ForceMode.Impulse);
                }
                if (bodyPart.CompareTag("Head"))
                {
                    health = Mathf.Max(health - damage, 0);
                }
                else if (bodyPart.CompareTag("Upper Body"))
                {
                    upperBodyDamage += damage;
                }
                else if (bodyPart.CompareTag("Lower Body"))
                {
                    lowerBodyDamage += damage;
                }

                UpdateAnimatorDamage();

                if (health > 0)
                {

                }
            }
        }

        bool shouldRagDoll = hitStrength > 1.0f;

        if (health <= 0) shouldRagDoll = true;

        if (shouldRagDoll)
        {
            if (currentState)
            {
                currentState.OnEnterState();
                currentState = null;
                currentStateType = AIStateType.None;
            }

            if (navAgent) navAgent.enabled = false;
            if (animator) animator.enabled = false;
            if (coll) coll.enabled = false;

            InMeleeRange = false;

            foreach (Rigidbody body in bodyParts)
            {
                if (body)
                    body.isKinematic = false;
            }

            if (hitStrength > 1.0f)
            {
                bodyPart.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
