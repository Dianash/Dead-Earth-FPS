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

    public void Set(AITargetType type, Collider collider, Vector3 position, float distance)
    {
        this.type = type;
        this.collider = collider;
        this.position = position;
        this.distance = distance;

        time = UnityEngine.Time.time;
    }

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

    protected Dictionary<AIStateType, AIState> states = new Dictionary<AIStateType, AIState>();
    protected AITarget target = new AITarget();

    [SerializeField]
    protected SphereCollider targetTrigger = null;
    [SerializeField]
    protected SphereCollider sensorTrigger = null;

    //Component cache
    protected Animator animator = null;
    protected NavMeshAgent navAgent = null;
    protected Collider agentCollider = null;
    protected Transform transform = null;

    public Animator Animator { get => animator; }
    public NavMeshAgent NavAgent { get => navAgent; }


    protected virtual void Start()
    {
        //Fetch all states on this game object
        AIState[] AIstates = GetComponents<AIState>();

        foreach (AIState state in AIstates)
        {
            if (state != null && !states.ContainsKey(state.GetStateType()))
            {
                AIstates[(int)state.GetStateType()] = state;
            }
        }
    }
}
