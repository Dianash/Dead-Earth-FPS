using UnityEngine;

/// <summary>
/// The base class of all AI States used by AI System
/// </summary>
public abstract class AIState : MonoBehaviour
{
    protected AIStateMachine stateMachine;

    /// <summary>
    /// Called by the parent state machine to assign its reference
    /// </summary>
    public virtual void SetStateMachine(AIStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void OnEnterState() { }

    public virtual void OnExitState() { }

    public virtual void OnAnimatorUpdated()
    {
        if (stateMachine.UseRootPosition)
            stateMachine.NavAgent.velocity = stateMachine.Animator.deltaPosition / Time.deltaTime;

        if (stateMachine.UseRootPosition)
            stateMachine.transform.rotation = stateMachine.Animator.rootRotation;
    }

    public virtual void OnAnimatorIKUpdated() { }

    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }

    public virtual void OnDestinationtReached(bool isReached) { }

    public abstract AIStateType GetStateType();

    public abstract AIStateType OnUpdate();

    /// <summary>
    /// Converts the passed sphere collider`s position and radius into world space taking into acount
    /// hierarchical scaling.
    /// </summary>
    public static void ConvertSphereColliderToWorldSpace(SphereCollider collider, out Vector3 position, out float radius)
    {
        position = Vector3.zero;
        radius = 0.0f;

        if (collider == null) return;

        // Calculate world space position of sphere center
        position = collider.transform.position;

        position.x += collider.center.x * collider.transform.lossyScale.x;
        position.y += collider.center.y * collider.transform.lossyScale.y;
        position.z += collider.center.z * collider.transform.lossyScale.z;

        // Calculate world space radius of sphere
        radius = Mathf.Max(collider.radius * collider.transform.lossyScale.x,
                                  collider.radius * collider.transform.lossyScale.y);

        radius = Mathf.Max(radius, collider.radius * collider.transform.lossyScale.z);
    }

    /// <summary>
    /// Calculates the signed angle between two vectors.
    /// </summary>
    /// <returns> 
    /// Returnes angle in degrees.
    /// </returns>
    public static float FindSignedAngle(Vector3 fromVector, Vector3 toVector)
    {
        if (fromVector == toVector)
            return 0.0f;

        float angle = Vector3.Angle(fromVector, toVector);
        Vector3 cross = Vector3.Cross(fromVector, toVector);
        angle *= Mathf.Sign(cross.y);

        return angle;
    }
}
