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


}
