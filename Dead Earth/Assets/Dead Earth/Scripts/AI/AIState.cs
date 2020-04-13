using UnityEngine;

public abstract class AIState : MonoBehaviour
{
    protected AIStateMachine stateMachine;

    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationtReached(bool isReached) { }

    public void SetStateMachine(AIStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();
}
