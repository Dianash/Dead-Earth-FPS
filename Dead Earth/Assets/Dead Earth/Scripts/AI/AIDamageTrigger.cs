using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string parameter = null;

    private AIStateMachine stateMachine = null;
    private Animator animator = null;
    private int parameterHash = -1;

    private void Start()
    {
        stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (stateMachine != null)
            animator = stateMachine.Animator;

        parameterHash = Animator.StringToHash(parameter);
    }

    private void OnTriggerStay(Collider collider)
    {
        if (!animator)
            return;

        if (collider.gameObject.CompareTag("Player") && animator.GetFloat(parameterHash) > 0.9f)
        {
            Debug.Log("Player being damaged");
        }
    }
}
