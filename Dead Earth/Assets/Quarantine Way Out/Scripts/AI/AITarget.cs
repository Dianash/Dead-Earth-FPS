using UnityEngine;

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
