using UnityEngine;

/// <summary>
/// Stores information about the position of each body part when transitioning from a ragdoll
/// </summary>
public class BodyPartSnapshot
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion localRotation;
}
