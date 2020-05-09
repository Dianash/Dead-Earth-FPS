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
}
