using UnityEngine;

public class AISensor : MonoBehaviour
{
    private AIStateMachine parentStateMachine = null;

    public AIStateMachine ParentStateMachine
    {
        get => parentStateMachine;
        set => parentStateMachine = value;
    }
}
