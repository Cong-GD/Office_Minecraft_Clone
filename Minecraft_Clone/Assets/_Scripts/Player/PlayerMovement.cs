using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float accelerate = 0.098f;

    [SerializeField] float playerSpeed = 4.317f;


    [SerializeField] float jumpHeight = 1.2522f;
    [SerializeField] float gravityValue = -9.81f;
    [SerializeField] float upDownSpeed = 2f;


    [Header("Grounded check parameters:")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float rayDistance = 1;

    public float currentSpeed;
    private CharacterController controller;
    private Vector3 playerVelocity;

    [field: SerializeField]
    public bool IsGrounded { get; private set; }

    public float SprintingSpeed => playerSpeed * 1.3f;

    public float SneekingSpeed => playerSpeed * 0.3f;

    public float FlyingSpeed => playerSpeed * 2.5f;

    public float SprintFlying => playerSpeed * 5f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        currentSpeed = playerVelocity.magnitude;
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * rayDistance);
    }

    Vector3 GetMovementDirection(Vector3 movementInput)
    {
        return (transform.right * movementInput.x + transform.forward * movementInput.z).normalized;
    }

    public void Fly(Vector3 movementInput, bool ascendInput, bool descendInput)
    {
        Vector3 movementDirection = GetMovementDirection(movementInput);

        if (ascendInput)
        {
            movementDirection += Vector3.up * upDownSpeed;
        }
        else if (descendInput)
        {
            movementDirection -= Vector3.up * upDownSpeed;
        }
        controller.Move(playerSpeed * 2f * Time.deltaTime * movementDirection);
    }
    public void Walk(Vector3 moveInput, bool runningInput)
    {
        Vector3 moveDirection = GetMovementDirection(moveInput);
        float speed = runningInput ? SprintingSpeed : playerSpeed;
        controller.Move(speed * Time.deltaTime * moveDirection);
    }

    public void HandleGravity(bool isJumping)
    {
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        if (isJumping && IsGrounded)
            AddJumpForce();
        ApplyGravityForce();
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void ApplyGravityForce()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        playerVelocity.y = Mathf.Clamp(playerVelocity.y, gravityValue, 10);
    }

    void AddJumpForce()
    {
        playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
    }

}