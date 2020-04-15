using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public enum AIStateType
{
    None,
    Idle,
    Alerted,
    Patrol,
    Attack,
    Feeding,
    Pursuit,
    Dead
}

public enum AITargetType
{
    None,
    Waypoint,
    VisualPlayer,
    VisualLight,
    VisualFood,
    Audio
}

public enum AITriggerEventType
{
    Enter,
    Stay,
    Exit
}


/// <summary>
/// Describes a potential target to the AI System
/// </summary>
public struct AITarget
{
    private AITargetType type;
    private Collider collider;
    private Vector3 position;
    private float distance;
    private float time;

    public AITargetType Type { get => type; }
    public Collider Collider { get => collider; }
    public Vector3 Position { get => position; }
    public float Time { get => time; }

    public float Distance
    {
        get => distance;
        set => distance = value;
    }

    /// <summary>
    /// Sets the target data
    /// </summary>
    public void Set(AITargetType type, Collider collider, Vector3 position, float distance)
    {
        this.type = type;
        this.collider = collider;
        this.position = position;
        this.distance = distance;

        time = UnityEngine.Time.time;
    }

    /// <summary>
    /// Clears the target data
    /// </summary>
    public void Clear()
    {
        type = AITargetType.None;
        collider = null;
        position = Vector3.zero;
        time = 0.0f;
        distance = Mathf.Infinity;
    }
}

/// <summary>
/// Base class for all AI State Machines
/// </summary>
public abstract class AIStateMachine : MonoBehaviour
{
    public AITarget visualThreat = new AITarget();
    public AITarget audioThreat = new AITarget();

    public Animator Animator { get => animator; }
    public NavMeshAgent NavAgent { get => navAgent; }

    protected Dictionary<AIStateType, AIState> states = new Dictionary<AIStateType, AIState>();
    protected AITarget target = new AITarget();
    protected AIState currentState = null;

    [SerializeField]
    protected AIStateType currentStateType = AIStateType.Idle;

    [SerializeField]
    protected SphereCollider targetTrigger = null;

    [SerializeField]
    protected SphereCollider sensorTrigger = null;

    [SerializeField]
    [Range(0, 15)]
    protected float stoppingDistance = 1.0f;

    // Component cache
    protected Animator animator = null;
    protected NavMeshAgent navAgent = null;
    protected Collider objectCollider = null;
    protected Transform objectTransform = null;

    /// <summary>
    /// Caches Components
    /// </summary>
    protected virtual void Awake()
    {
        // Cache all frequently accessed components
        objectTransform = transform;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        objectCollider = GetComponent<Collider>();

        if (GameSceneManager.Instance != null)
        {
            if (objectCollider)
                GameSceneManager.Instance.RegisterAIStateMachine(objectCollider.GetInstanceID(), this);

            if (sensorTrigger)
                GameSceneManager.Instance.RegisterAIStateMachine(sensorTrigger.GetInstanceID(), this);
        }
    }

    protected virtual void Start()
    {
        // Fetch all states on this game object
        AIState[] AIstates = GetComponents<AIState>();

        foreach (AIState state in AIstates)
        {
            if (state != null && !states.ContainsKey(state.GetStateType()))
            {
                AIstates[(int)state.GetStateType()] = state;
                state.SetStateMachine(this);
            }
        }

        if (states.ContainsKey(currentStateType))
        {
            currentState = states[currentStateType];
            currentState.OnEnterState();
        }
        else
        {
            currentState = null;
        }
    }

    /// <summary>
    /// Sets the current target and configures the target trigger
    /// </summary>
    public void SetTarget(AITargetType type, Collider collider, Vector3 position, float distance)
    {
        target.Set(type, collider, position, distance);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = target.Position;
            targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Sets the current target and configures the target trigger.
    /// This method allows specifying a custom stopping distance.
    /// </summary>
    public void SetTarget(AITargetType type, Collider collider, Vector3 position, float distance, float stoppingDistance)
    {
        target.Set(type, collider, position, distance);

        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = target.Position;
            targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Sets the current target and configures the target trigger
    /// </summary>
    public void SetTarget(AITarget type)
    {
        target = type;

        if (targetTrigger != null)
        {
            targetTrigger.radius = stoppingDistance;
            targetTrigger.transform.position = target.Position;
            targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Clears the current target
    /// </summary>
    public void Clear()
    {
        target.Clear();

        if (targetTrigger != null)
        {
            targetTrigger.enabled = false;
        }
    }

    /// <summary>
    /// Clears the audio and visual threats each update and	re-calculates the distance to the current target
    /// </summary>
    protected virtual void FixedUpdate()
    {
        visualThreat.Clear();
        audioThreat.Clear();

        if (target.Type != AITargetType.None)
        {
            target.Distance = Vector3.Distance(objectTransform.position, target.Position);
        }
    }

    /// <summary>
    /// Gives the current state a chance to update itself and perform transitions.
    /// </summary>
    protected virtual void Update()
    {
        if (currentState == null)
            return;

        AIStateType newStateType = currentState.OnUpdate();

        if (newStateType != currentStateType)
        {
            AIState newState = null;

            if (states.TryGetValue(newStateType, out newState))
            {
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }
            else if (states.TryGetValue(AIStateType.Idle, out newState))
            {
                currentState.OnExitState();
                newState.OnEnterState();
                currentState = newState;
            }
        }
        currentStateType = newStateType;
    }

    /// <summary>
    /// Called by Physics system when the AI`s Main collider enters its trigger, that allows state 
    /// to know when it has entered the sphere of influence of a waypoint or last player sighted position
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger)
            return;

        // Notify Child State
        if (currentState)
            currentState.OnDestinationtReached(true);
    }

    /// <summary>
    /// Informs the child state that the AI entity is no longer at its destination 
    /// (typically true when a new target has been set by the child)
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger)
            return;

        if (currentState)
            currentState.OnDestinationtReached(false);
    }

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (currentState)
            currentState.OnTriggerEvent(type, other);
    }
}
