﻿using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private AudioCollection footsteps = null;

    [SerializeField] private float crouchAttenuation = 0.2f;

    [SerializeField] private float walkSpeed = 2.0f;

    [SerializeField] private float runSpeed = 4.5f;

    [SerializeField] private float jumpSpeed = 7.5f;

    [SerializeField] private float crouchSpeed = 1.0f;

    [SerializeField] private float staminaDepletion = 5.0f;

    [SerializeField] private float staminaRecovery = 10;

    [SerializeField] private float stickToGroundForce = 5.0f;

    [SerializeField] private float gravityMultyplier = 2.5f;

    [SerializeField] private float fallingTimerThreshold = 0.5f;

    [SerializeField] private float runStepLenghten = 0.75f;

    [SerializeField] private GameObject flashLight = null;

    [SerializeField] private bool flashlightOnAtStart = true;

    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();

    [SerializeField] [Range(0.0f, 1.0f)] float npcStickiness = 0.5f;

    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;

    private Camera cam = null;
    private Vector2 inputVector = Vector2.zero;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 localSpaceCameraPos = Vector3.zero;

    private bool jumpButtonPressed = false;
    private bool previouslyGrounded = false;
    private bool isWalking = true;
    private bool isJumping = false;
    private bool isCrouching = false;

    private float controllerHeight = 0.0f;
    private float fallingTimer = 0.0f;

    private float stamina = 100;
    private bool freezeMovement = false;

    private CharacterController characterController = null;
    private PlayerMoveStatus movementStatus = PlayerMoveStatus.NotMoving;

    public PlayerMoveStatus MovementStatus { get => movementStatus; }
    public float WalkSpeed { get => walkSpeed; }
    public float RunSpeed { get => runSpeed; }

    private float dragMultiplier = 1.0f;
    private float dragMultiplierLimit = 1.0f;

    public float DragMultiplierLimit
    {
        get
        {
            return dragMultiplierLimit;
        }
        set
        {
            dragMultiplierLimit = Mathf.Clamp01(value);
        }
    }

    public float DragMultiplier
    {
        get
        {
            return dragMultiplier;
        }
        set
        {
            dragMultiplier = Mathf.Min(value, dragMultiplierLimit);
        }
    }

    public CharacterController CharacterController { get => characterController; }

    public float Stamina { get => stamina; }

    public bool FreezeMovement { get => freezeMovement; set => freezeMovement = value; }

    public void DoStickiness()
    {
        dragMultiplier = 1.0f - npcStickiness;
    }

    protected void Start()
    {
        characterController = GetComponent<CharacterController>();
        controllerHeight = characterController.height;
        cam = Camera.main;
        localSpaceCameraPos = cam.transform.localPosition;
        movementStatus = PlayerMoveStatus.NotMoving;
        fallingTimer = 0.0f;
        mouseLook.Init(transform, cam.transform);
        headBob.Initialize();
        headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);

        if (flashLight)
            flashLight.SetActive(flashlightOnAtStart);
    }

    protected void Update()
    {
        if (characterController.isGrounded)
            fallingTimer = 0.0f;
        else
            fallingTimer += Time.deltaTime;

        // Allow Mouse Look a chance to process mouse and rotate camera
        if (Time.timeScale > Mathf.Epsilon)
        {
            mouseLook.LookRotation(transform, cam.transform);
        }

        if (Input.GetButtonDown("FlashLight"))
        {
            if (flashLight)
                flashLight.SetActive(!flashLight.activeInHierarchy);
        }

        // Process the Jump Button
        if (!jumpButtonPressed && !isCrouching)
            jumpButtonPressed = Input.GetButtonDown("Jump");

        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;
            characterController.height = isCrouching == true ? controllerHeight / 2.0f : controllerHeight;
        }

        if (!previouslyGrounded && characterController.isGrounded)
        {
            if (fallingTimer > 0.5f)
            {
                // TODO: Play landing sound
            }

            moveDirection.y = 0f;
            isJumping = false;
            movementStatus = PlayerMoveStatus.Landing;
        }
        else if (!characterController.isGrounded)
            movementStatus = PlayerMoveStatus.NotGrounded;
        else if (characterController.velocity.sqrMagnitude < 0.01f)
            movementStatus = PlayerMoveStatus.NotMoving;
        else if (isCrouching)
            movementStatus = PlayerMoveStatus.Crouching;
        else if (isWalking)
            movementStatus = PlayerMoveStatus.Walking;
        else
            movementStatus = PlayerMoveStatus.Running;

        previouslyGrounded = characterController.isGrounded;

        // Calculate Stamina
        if (movementStatus == PlayerMoveStatus.Running)
            stamina = Mathf.Max(stamina - staminaDepletion * Time.deltaTime, 0.0f);
        else
            stamina = Mathf.Min(stamina + staminaRecovery * Time.deltaTime, 100.0f);

        dragMultiplier = Mathf.Min(dragMultiplier + Time.deltaTime, dragMultiplierLimit);
    }

    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool wasWalking = isWalking;
        isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = isCrouching ? crouchSpeed : isWalking ? walkSpeed : Mathf.Lerp(walkSpeed, runSpeed, stamina / 100.0f);
        inputVector = new Vector2(horizontal, vertical);

        if (inputVector.sqrMagnitude > 1) inputVector.Normalize();

        // Always move along the camera forward as it is direction that is being aimed at
        Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

        // Get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f, 1))
        {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        }

        // Scale movement by our current speed (walking or running)
        moveDirection.x = !freezeMovement ? desiredMove.x * speed * dragMultiplier : 0.0f;
        moveDirection.z = !freezeMovement ? desiredMove.z * speed * dragMultiplier : 0.0f;

        if (characterController.isGrounded)
        {
            // Apply severe down force to keep control sticking to floor
            moveDirection.y = -stickToGroundForce;

            if (jumpButtonPressed)
            {
                moveDirection.y = jumpSpeed;
                jumpButtonPressed = false;
                isJumping = true;
            }
        }
        else
        {
            // Apply standart system gravity multiplied by our gravity modifier
            moveDirection += Physics.gravity * gravityMultyplier * Time.fixedDeltaTime;
        }

        characterController.Move(moveDirection * Time.fixedDeltaTime);

        // Are we moving
        Vector3 speedXZ = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z);
        if (speedXZ.magnitude > 0.01f)
            cam.transform.localPosition = localSpaceCameraPos + headBob.GetVectorOffset(speedXZ.magnitude * (isCrouching || isWalking ? 1.0f : runStepLenghten));
        else
            cam.transform.localPosition = localSpaceCameraPos;
    }

    private void PlayFootStepSound()
    {
        if (AudioManager.Instance != null && footsteps != null)
        {
            AudioClip soundToPlay;

            if (isCrouching)
                soundToPlay = footsteps[1];
            else
                soundToPlay = footsteps[0];

            AudioManager.Instance.PlayOneShotSound("Player", soundToPlay, transform.position, isCrouching ? footsteps.Volume * crouchAttenuation : footsteps.Volume,
                footsteps.SpatialBlend, footsteps.Priority);
        }
    }
}
