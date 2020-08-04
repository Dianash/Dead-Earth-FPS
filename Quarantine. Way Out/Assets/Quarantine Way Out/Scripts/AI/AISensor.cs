using UnityEngine;

public class AISensor : MonoBehaviour
{
    private AIStateMachine parentStateMachine = null;

    public AIStateMachine ParentStateMachine
    {
        set => parentStateMachine = value;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, collider);
    }

    private void OnTriggerStay(Collider collider)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, collider);

    }

    private void OnTriggerExit(Collider collider)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, collider);
    }
}
