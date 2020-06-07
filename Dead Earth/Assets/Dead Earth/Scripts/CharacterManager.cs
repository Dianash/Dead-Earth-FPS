using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera cam = null;
    [SerializeField] private float health = 100.0f;
    [SerializeField] private AISoundEmitter soundEmitter = null;
    [SerializeField] private float walkRadius = 0.0f;
    [SerializeField] private float runRadius = 7.0f;
    [SerializeField] private float landingRadius = 12.0f;
    [SerializeField] private float bloodRadiusScale = 6.0f;

    [SerializeField] private AudioCollection damageSounds = null;
    [SerializeField] private AudioCollection painSounds = null;
    [SerializeField] private float nextPainSoundTime = 0.0f;
    [SerializeField] private float painSoundOffset = 0.35f;

    private Collider coll = null;
    private FPSController fPSController = null;
    private CharacterController characterController = null;
    private GameSceneManager gameSceneManager = null;
    private int aiBodyPartLayer = -1;

    private void Start()
    {
        coll = GetComponent<Collider>();
        fPSController = GetComponent<FPSController>();
        characterController = GetComponent<CharacterController>();
        gameSceneManager = GameSceneManager.Instance;

        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = cam;
            info.characterManager = this;
            info.collider = coll;
            info.meleeTrigger = meleeTrigger;

            gameSceneManager.RegisterPlayerInfo(coll.GetInstanceID(), info);
        }
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - (amount * Time.deltaTime), 0.0f);

        if (fPSController)
        {
            fPSController.DragMultiplier = 0.0f;
        }

        if (cameraBloodEffect != null)
        {
            cameraBloodEffect.MinBloodAmount = 1.0f - health / 100.0f;
            cameraBloodEffect.BloodAmount = Mathf.Min(cameraBloodEffect.MinBloodAmount + 0.3f, 1.0f);
        }
    }

    public void DoDamage(int hitDirection = 0)
    {
        if (cam == null) return;

        Ray ray;
        RaycastHit hit;
        bool isSomethingHit = false;

        ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        isSomethingHit = Physics.Raycast(ray, out hit, 1000.0f, 1 << aiBodyPartLayer);

        if (isSomethingHit)
        {
            AIStateMachine stateMachine = gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());

            if (stateMachine)
            {
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 50, hit.rigidbody, this, 0);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }

        if (fPSController && (soundEmitter != null))
        {
            float newRadius = Mathf.Max(walkRadius, (100.0f - health) / bloodRadiusScale);
            switch (fPSController.MovementStatus)
            {
                case PlayerMoveStatus.Landing:
                    newRadius = Mathf.Max(newRadius, landingRadius);
                    break;
                case PlayerMoveStatus.Running:
                    newRadius = Mathf.Max(newRadius, runRadius);
                    break;
            }

            soundEmitter.SetRadius(newRadius);
            fPSController.DragMultiplierLimit = Mathf.Max(health / 100.0f, 0.25f);
        }
    }
}
