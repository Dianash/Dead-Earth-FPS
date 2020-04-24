using UnityEngine;

public class AIZombieStateMachine : AIStateMachine
{
    #region Serialized fields

    [SerializeField] [Range(10.0f, 360.0f)] float fov = 50.0f;

    [SerializeField] [Range(0.0f, 1.0f)] float sight = 0.5f;

    [SerializeField] [Range(0.0f, 1.0f)] float hearing = 50.0f;

    [SerializeField] [Range(0.0f, 1.0f)] float aggression = 0.5f;

    [SerializeField] [Range(0, 100)] int health = 100;

    [SerializeField] [Range(0.0f, 1.0f)] float intelligence = 0.5f;

    [SerializeField] [Range(0.0f, 1.0f)] float satisfaction = 1f;

    #endregion

    #region Hashes

    private int speedHash = Animator.StringToHash("Speed");
    private int seekingHash = Animator.StringToHash("Seeking");
    private int feedingHash = Animator.StringToHash("Feeding");
    private int attackHash = Animator.StringToHash("Attack");

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

    public float Speed
    {
        get
        {
            return navAgent != null ? navAgent.speed : 0.0f;
        }
        set
        {
            if (navAgent != null)
                navAgent.speed = value;
        }
    }

    #endregion

    /// <summary>
    /// Refreshes the animator with up-to-date values for its parameters 
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (animator != null)
        {
            animator.SetFloat(speedHash, navAgent.speed);
            animator.SetBool(feedingHash, Feeding);
            animator.SetInteger(seekingHash, Seeking);
            animator.SetInteger(attackHash, AttackType);
        }
    }
}
