using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CapsuleCollider meleeTrigger = null;
    [SerializeField] private CameraBloodEffect cameraBloodEffect = null;
    [SerializeField] private Camera cam = null;
    [SerializeField] private float health = 100.0f;

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
                stateMachine.TakeDamage(hit.point, ray.direction * 35.0f, 35, hit.rigidbody, this, 0);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DoDamage();
        }
    }
}
