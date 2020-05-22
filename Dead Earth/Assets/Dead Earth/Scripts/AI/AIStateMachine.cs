using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// Base class for all AI State Machines
/// </summary>
public abstract class AIStateMachine : MonoBehaviour
{
    public AITarget visualThreat = new AITarget();
    public AITarget audioThreat = new AITarget();

    protected Dictionary<AIStateType, AIState> states = new Dictionary<AIStateType, AIState>();
    protected AITarget target = new AITarget();
    protected AIState currentState = null;

    protected int rootPositionRefCount = 0;
    protected int rootRotationRefCount = 0;
    protected bool isTargetReached = false;
    protected List<Rigidbody> bodyParts = new List<Rigidbody>();
    protected int aiBodyPartLayer = -1;
    protected bool cinematicEnabled = false;

    [SerializeField] protected AIStateType currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider targetTrigger = null;
    [SerializeField] protected SphereCollider sensorTrigger = null;
    [SerializeField] protected AIWaypointNetwork waypointNetwork = null;
    [SerializeField] protected bool randomPatrol = false;
    [SerializeField] protected int currentWaypoint = -1;
    [SerializeField] [Range(0, 15)] protected float stoppingDistance = 1.0f;

    [SerializeField] protected Transform rootBone = null;
    [SerializeField] protected AIBoneAlignmentType rootBoneAlingment = AIBoneAlignmentType.ZAxis;

    // Component cache
    protected Animator animator = null;
    protected NavMeshAgent navAgent = null;
    protected Collider coll = null;
    protected Transform objectTransform = null;

    public Animator Animator { get => animator; }
    public NavMeshAgent NavAgent { get => navAgent; }
    public bool InMeleeRange { get; set; }

    public Vector3 SensorPosition
    {
        get
        {
            if (sensorTrigger == null)
                return Vector3.zero;

            Vector3 point = sensorTrigger.transform.position;
            point.x += sensorTrigger.center.x * sensorTrigger.transform.lossyScale.x;
            point.y += sensorTrigger.center.y * sensorTrigger.transform.lossyScale.y;
            point.z += sensorTrigger.center.z * sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float SensorRadius
    {
        get
        {
            if (sensorTrigger == null)
                return 0.0f;

            float radius = Mathf.Max(sensorTrigger.radius * sensorTrigger.transform.lossyScale.x,
                                     sensorTrigger.radius * sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(radius, sensorTrigger.radius * sensorTrigger.transform.lossyScale.z);
        }
    }

    public bool UseRootPosition
    {
        get
        {
            return rootPositionRefCount > 0;
        }
    }

    public bool UseRootRotation
    {
        get
        {
            return rootRotationRefCount > 0;
        }
    }

    public AITargetType TargetType
    {
        get
        {
            return target.Type;
        }
    }

    public Vector3 TargetPosition
    {
        get
        {
            return target.Position;
        }
    }

    public int TargetColliderID
    {
        get
        {
            if (target.Collider)
                return target.Collider.GetInstanceID();
            else
                return -1;
        }
    }

    public bool IsTargetReached
    {
        get
        {
            return isTargetReached;
        }
    }

    public bool CinematicEnabled { get => cinematicEnabled; set => cinematicEnabled = value; }

    /// <summary>
    /// Fetches the world space position of the state machine`s currently set waypoint.
    /// </summary>
    /// <param name="increment"> 
    /// Optional increment of waypoint index.
    /// </param>
    /// <returns>
    /// Vector3 world space position of current waypoint.
    /// </returns>
    public Vector3 GetWaypointPosition(bool increment)
    {
        if (currentWaypoint == -1)
        {
            if (randomPatrol)
                currentWaypoint = Random.Range(0, waypointNetwork.waypoints.Count);
            else
                currentWaypoint = 0;
        }
        else if (increment)
            NextWaipoint();

        if (waypointNetwork.waypoints[currentWaypoint] != null)
        {
            Transform newWaypoint = waypointNetwork.waypoints[currentWaypoint];
            SetTarget(AITargetType.Waypoint, null, newWaypoint.position, Vector3.Distance(newWaypoint.position, transform.position));

            return newWaypoint.position;
        }

        return Vector3.zero;
    }


    private void NextWaipoint()
    {
        if (randomPatrol && waypointNetwork.waypoints.Count > 1)
        {
            int oldWaypoint = currentWaypoint;
            while (currentWaypoint == oldWaypoint)
            {
                currentWaypoint = Random.Range(0, waypointNetwork.waypoints.Count);
            }
        }
        else
            currentWaypoint = currentWaypoint == waypointNetwork.waypoints.Count - 1 ? 0 : currentWaypoint + 1;


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

    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (currentState)
            currentState.OnTriggerEvent(type, other);
    }

    /// <summary>
    /// Clears the current target
    /// </summary>
    public void ClearTarget()
    {
        target.Clear();

        if (targetTrigger != null)
        {
            targetTrigger.enabled = false;
        }
    }

    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (navAgent)
        {
            navAgent.updatePosition = positionUpdate;
            navAgent.updateRotation = rotationUpdate;
        }
    }

    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        rootPositionRefCount += rootPosition;
        rootRotationRefCount += rootRotation;
    }

    /// <summary>
    /// Caches Components
    /// </summary>
    protected virtual void Awake()
    {
        // Cache all frequently accessed components
        objectTransform = transform;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        coll = GetComponent<Collider>();
        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (GameSceneManager.Instance != null)
        {
            if (coll)
                GameSceneManager.Instance.RegisterAIStateMachine(coll.GetInstanceID(), this);

            if (sensorTrigger)
                GameSceneManager.Instance.RegisterAIStateMachine(sensorTrigger.GetInstanceID(), this);
        }

        if (rootBone != null)
        {
            Rigidbody[] bodies = rootBone.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody bodyPart in bodies)
            {
                if (bodyPart != null && bodyPart.gameObject.layer == aiBodyPartLayer)
                {
                    bodyParts.Add(bodyPart);
                    GameSceneManager.Instance.RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
                }
            }
        }
    }

    protected virtual void Start()
    {
        if (sensorTrigger != null)
        {
            AISensor AISensor = sensorTrigger.GetComponent<AISensor>();

            if (AISensor != null)
            {
                AISensor.ParentStateMachine = this;
            }
        }

        // Fetch all states on this game object
        AIState[] AIstates = GetComponents<AIState>();

        foreach (AIState state in AIstates)
        {
            if (state != null && !states.ContainsKey(state.GetStateType()))
            {
                states[state.GetStateType()] = state;
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

        if (animator)
        {
            AIStateMachineLink[] scripts = animator.GetBehaviours<AIStateMachineLink>();

            foreach (AIStateMachineLink script in scripts)
            {
                script.StateMachine = this;
            }
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

        isTargetReached = false;
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

        isTargetReached = true;

        // Notify Child State
        if (currentState)
            currentState.OnDestinationtReached(true);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (targetTrigger == null || other != targetTrigger)
            return;

        isTargetReached = true;
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

    protected virtual void OnAnimatorMove()
    {
        if (currentState != null)
            currentState.OnAnimatorUpdated();
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (currentState != null)
            currentState.OnAnimatorIKUpdated();
    }

    public virtual void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager character, int hitDirection) { }
}
