using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 1.0f;

    [SerializeField] private float runSpeed = 4.5f;

    [SerializeField] private float jumpSpeed = 7.5f;

    [SerializeField] private float stickToGroundForce = 5.0f;

    [SerializeField] private float gravityMultyplier = 2.5f;

    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook;

    private Camera camera = null;
    private Vector3 inputVector = Vector3.zero;
    private Vector3 moveDirection = Vector3.zero;

    private bool jumpButtonPressed = false;
    private bool previouslyGrounded = false;
    private bool isWalking = true;
    private bool isJumping = false;

    private float fallingTimer = 0.0f;

    private CharacterController characterController = null;
    private PlayerMoveStatus movementStatus = PlayerMoveStatus.NotMoving;

    public PlayerMoveStatus MovementStatus { get => movementStatus; }
    public float WalkSpeed { get => walkSpeed; }
    public float RunSpeed { get => runSpeed; }

    protected void Start()
    {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;
        movementStatus = PlayerMoveStatus.NotMoving;
        fallingTimer = 0.0f;
        mouseLook.Init(transform, camera.transform);
    }

    protected void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool wasWalking = isWalking;
        isWalking = !Input.GetKey(KeyCode.LeftShift);

        float speed = isWalking ? walkSpeed : runSpeed;
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
        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

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
    }


}
