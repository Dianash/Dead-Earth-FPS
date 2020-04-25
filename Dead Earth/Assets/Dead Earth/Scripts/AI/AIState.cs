using UnityEngine;

/// <summary>
/// The base class of all AI States used by AI System
/// </summary>
public abstract class AIState : MonoBehaviour
{
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

    protected AIStateMachine stateMachine;
}
