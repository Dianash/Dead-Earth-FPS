using UnityEngine;

public class MeleeZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        AIStateMachine machine = GameSceneManager.Instance.GetAIStateMachine(collider.GetInstanceID());
        if (machine)
        {
            machine.InMeleeRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        AIStateMachine machine = GameSceneManager.Instance.GetAIStateMachine(collider.GetInstanceID());
        if (machine)
        {
            machine.InMeleeRange = false;
        }
    }
}
