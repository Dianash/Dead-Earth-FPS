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
    [SerializeField] private AudioCollection tauntSounds = null;
    [SerializeField] private float nextPainSoundTime = 0.0f;
    [SerializeField] private float painSoundOffset = 0.35f;
    [SerializeField] private float tauntRadius = 10.0f;
    [SerializeField] private PlayerHUD playerHUD = null;

    private Collider coll = null;
    private FPSController fpsController = null;
    private CharacterController characterController = null;
    private GameSceneManager gameSceneManager = null;
    private int aiBodyPartLayer = -1;
    private int interactiveMask = 0;
    private float nextAtackTime = 0;
    private float nextTauntSoundTime = 0.0f;

    public float Health { get => health; }

    public float Stamina { get => fpsController != null ? fpsController.Stamina : 0.0f; }

    public FPSController FPSController { get => fpsController; }

    private void Start()
    {
        coll = GetComponent<Collider>();
        fpsController = GetComponent<FPSController>();
        characterController = GetComponent<CharacterController>();
        gameSceneManager = GameSceneManager.Instance;

        aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        interactiveMask = 1 << LayerMask.NameToLayer("Interactive");

        if (gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = cam;
            info.characterManager = this;
            info.collider = coll;
            info.meleeTrigger = meleeTrigger;

            gameSceneManager.RegisterPlayerInfo(coll.GetInstanceID(), info);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerHUD)
            playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);
    }

    public void TakeDamage(float amount, bool doDamage, bool doPain)
    {
        health = Mathf.Max(health - (amount * Time.deltaTime), 0.0f);

        if (fpsController)
        {
            fpsController.DragMultiplier = 0.0f;
        }

        if (cameraBloodEffect != null)
        {
            cameraBloodEffect.MinBloodAmount = (1.0f - health / 100.0f) * 0.5f;
            cameraBloodEffect.BloodAmount = Mathf.Min(cameraBloodEffect.MinBloodAmount + 0.3f, 1.0f);
        }

        if (AudioManager.Instance)
        {
            if (doDamage && damageSounds != null)
            {
                AudioManager.Instance.PlayOneShotSound(damageSounds.AudioGroup, damageSounds.AudioClip, transform.position, damageSounds.Volume,
                    damageSounds.SpatialBlend, damageSounds.Priority);
            }

            if (doPain && painSounds != null && nextPainSoundTime < Time.time)
            {
                AudioClip painClip = painSounds.AudioClip;

                if (painClip)
                {
                    nextPainSoundTime = Time.time + painClip.length;

                    StartCoroutine(AudioManager.Instance.PlayOneShotSoundDelayed(painSounds.AudioGroup, painClip, transform.position, painSounds.Volume,
                        painSounds.SpatialBlend, painSoundOffset, painSounds.Priority));
                }
            }
        }

        if (health <= 0.0f)
        {
            DoDeath();
        }
    }

    public void DoDamage(int hitDirection = 0)
    {
        if (cam == null) return;

        Ray ray;
        RaycastHit hit;
        bool isSomethingHit = false;

        ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        isSomethingHit = Physics.Raycast(ray, out hit, 1.0f, 1 << aiBodyPartLayer);

        if (isSomethingHit)
        {
            AIStateMachine stateMachine = gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());

            if (stateMachine)
            {
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 1, hit.rigidbody, this, 0);
                nextAtackTime = Time.time + 0.3f;
            }
        }
    }

    private void Update()
    {
        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;

        // PROCESS INTERACTIVE OBJECTS
        // Is the crosshair over a usuable item or descriptive item first get ray from centre of screen
        ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // Calculate Ray Length
        float rayLength = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(cam.transform.forward, Vector3.up)));

        // Cast Ray and collect ALL hits
        hits = Physics.RaycastAll(ray, rayLength, interactiveMask);

        // Process the hits for the one with the highest priorty
        if (hits.Length > 0)
        {
            // Used to record the index of the highest priorty
            int highestPriority = int.MinValue;
            InteractiveItem priorityObject = null;

            // Iterate through each hit
            for (int i = 0; i < hits.Length; i++)
            {
                // Process next hit
                hit = hits[i];

                // Fetch its InteractiveItem script from the database
                InteractiveItem interactiveObject = gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());

                // If this is the highest priority object so far then remember it
                if (interactiveObject != null && interactiveObject.Priority > highestPriority)
                {
                    priorityObject = interactiveObject;
                    highestPriority = priorityObject.Priority;
                }
            }

            // If we found an object then display its text and process any possible activation
            if (priorityObject != null)
            {
                if (playerHUD)
                    playerHUD.SetInteractionText(priorityObject.GetText());

                if (Input.GetButtonDown("Use"))
                {
                    priorityObject.Activate(this);
                }
            }
        }
        else
        {
            if (playerHUD)
                playerHUD.SetInteractionText(null);
        }

        if (Input.GetMouseButtonDown(0) && Time.time > nextAtackTime)
        {
            DoDamage();
        }

        if (fpsController && (soundEmitter != null))
        {
            float newRadius = Mathf.Max(walkRadius, (100.0f - health) / bloodRadiusScale);
            switch (fpsController.MovementStatus)
            {
                case PlayerMoveStatus.Landing:
                    newRadius = Mathf.Max(newRadius, landingRadius);
                    break;
                case PlayerMoveStatus.Running:
                    newRadius = Mathf.Max(newRadius, runRadius);
                    break;
            }

            soundEmitter.SetRadius(newRadius);
            fpsController.DragMultiplierLimit = Mathf.Max(health / 100.0f, 0.25f);
        }

        if (Input.GetMouseButtonDown(1))
        {
            DoTaunt();
        }

        if (playerHUD)
            playerHUD.Invalidate(this);
    }

    private void DoTaunt()
    {
        if (tauntSounds == null || Time.time < nextTauntSoundTime || !AudioManager.Instance)
            return;

        AudioClip taunt = tauntSounds[0];

        AudioManager.Instance.PlayOneShotSound(tauntSounds.AudioGroup, taunt, transform.position, tauntSounds.Volume,
            tauntSounds.SpatialBlend, tauntSounds.Priority);

        if (soundEmitter != null)
            soundEmitter.SetRadius(tauntRadius);

        nextTauntSoundTime = Time.time + taunt.length;
    }

    public void DoLevelComplete()
    {
        if (fpsController)
        {
            fpsController.FreezeMovement = true;
            fpsController.enabled = false;
        }

        if (playerHUD)
        {
            playerHUD.Fade(4.0f, ScreenFadeType.FadeOut);
            playerHUD.ShowMissionText("Mission Completed");
            playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 4.0f);
    }

    private void DoDeath()
    {
        if (fpsController)
        {
            fpsController.FreezeMovement = true;
        }


        if (playerHUD)
        {
            playerHUD.Fade(3.0f, ScreenFadeType.FadeOut);
            playerHUD.ShowMissionText("Mission Failed");
            playerHUD.Invalidate(this);
        }

        Invoke("GameOver", 3.0f);
    }

    public void GameOver()
    {
        // Show the cursor again
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (ApplicationManager.Instance)
            ApplicationManager.Instance.LoadMainMenu();
    }
}
