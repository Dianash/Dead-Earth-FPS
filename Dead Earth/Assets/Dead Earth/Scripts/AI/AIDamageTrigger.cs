using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string parameter = null;
    [SerializeField] int bloodParticlesBurstAmount = 10;

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
            if (GameSceneManager.Instance && GameSceneManager.Instance.BloodParticles)
            {
                ParticleSystem system = GameSceneManager.Instance.BloodParticles;

                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;
                ParticleSystemSimulationSpace spaceMode = system.main.simulationSpace;
                spaceMode = ParticleSystemSimulationSpace.World;
            }

            Debug.Log("Player being damaged");
        }
    }
}
