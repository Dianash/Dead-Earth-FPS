using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string parameter = null;
    [SerializeField] int bloodParticlesBurstAmount = 10;
    [SerializeField] float damageAmount = 0.1f;
    [SerializeField] bool doDamageSound = true;
    [SerializeField] bool doPainSound = true;

    private AIStateMachine stateMachine = null;
    private Animator animator = null;
    private int parameterHash = -1;
    private GameSceneManager gameSceneManager = null;
    private bool firstContact = false;

    private void Start()
    {
        stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (stateMachine != null)
            animator = stateMachine.Animator;

        parameterHash = Animator.StringToHash(parameter);

        gameSceneManager = GameSceneManager.Instance;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!animator)
            return;

        if (col.gameObject.CompareTag("Player") && animator.GetFloat(parameterHash) > 0.9f)
            firstContact = true;
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
        }

        if (gameSceneManager != null)
        {
            PlayerInfo info = gameSceneManager.GetPlayerInfo(collider.GetInstanceID());

            if (info != null && info.characterManager != null)
            {
                info.characterManager.TakeDamage(damageAmount, doDamageSound && firstContact, doPainSound);
            }
        }

        firstContact = false;
    }
}
