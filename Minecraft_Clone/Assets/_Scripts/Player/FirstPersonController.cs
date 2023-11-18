using Minecraft.Input;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField, Required]
    private Transform orientation;

    [field: SerializeField]
    public Rigidbody Rigidbody { get; private set; }

    [SerializeField]
    private PlayerData_SO data;
    private Vector2 _moveInput;

    private void OnEnable()
    {
        MInput.Move.performed += ProcessMoveInput;
        MInput.Move.canceled += ProcessMoveInput;

        MInput.Jump.performed += ProcessJumpInput;
        MInput.Sprint.performed += ProcessSprintInput;
        MInput.Crounch.performed += ProcessCronchInput;
        MInput.Crounch.canceled += ProcessCronchInput;
    }

    private void OnDisable()
    {
        MInput.Move.performed -= ProcessMoveInput;
        MInput.Move.canceled -= ProcessMoveInput;

        MInput.Jump.performed -= ProcessJumpInput;
        MInput.Sprint.performed -= ProcessSprintInput;
        MInput.Crounch.performed -= ProcessCronchInput;
        MInput.Crounch.canceled -= ProcessCronchInput;
    }

    private void Awake()
    {
        data.ClearTempData();
    }

    private void Update()
    {
        GroundCheck();
        ResetSprintState();
        ApplyVelocityDrag();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ProcessSprintInput(InputAction.CallbackContext context)
    {
        if (data.isCrounching)
            return;

        data.isSprinting = true;
    }

    private void ProcessMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    private void ProcessJumpInput(InputAction.CallbackContext context)
    {
        if (!data.isGrounded || Time.time < data.allowJumpTime)
            return;

        data.allowJumpTime = Time.time + data.JumpCooldown;
        Jump(1f);
    }

    private void ProcessCronchInput(InputAction.CallbackContext context)
    {
        data.isCrounching = context.performed;
        data.isSprinting &= data.isCrounching;
    }

    private void Move()
    {
        var moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
        float airMultilier = !data.isGrounded ? 10f * data.AirMultilier : 10f;
        float sprintMultilier = data.isSprinting ? data.SprintMultilier : 1f;
        float crounchMultilier = data.isCrounching && data.isGrounded ? data.CrounchMultilier : 1f;

        data.currentMoveSpeed = data.WalkSpeed * sprintMultilier * crounchMultilier;
        Rigidbody.AddForce(airMultilier * data.currentMoveSpeed * moveDirection, ForceMode.Force);
    }

    private void ResetSprintState()
    {
        data.isSprinting = data.isSprinting && _moveInput != Vector2.zero;
    }

    private void ApplyVelocityDrag()
    {
        Rigidbody.drag = data.isGrounded ? data.GroundDrag : data.AirDrag;
    }

    private void GroundCheck()
    {
        var position = Rigidbody.position;
        var spherePosition = position.X_Z(position.y + data.GroundOffset);
        data.isGrounded = Physics.CheckSphere(spherePosition, data.GroundRadius, data.GroundLayer, QueryTriggerInteraction.Ignore);
    }

    private void Jump(float value)
    {
        Rigidbody.velocity = Rigidbody.velocity.X_Z(0);
        Rigidbody.AddForce(data.JumpForce * value * transform.up, ForceMode.Impulse);
    }

    private void SpeedControl()
    {
        var flatVelocity = Rigidbody.velocity.X_Z(0);
        Rigidbody.velocity = Vector3.ClampMagnitude(flatVelocity, data.currentMoveSpeed).X_Z(Rigidbody.velocity.y);
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = data.isGrounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - data.GroundOffset, transform.position.z),
            data.GroundRadius);
    }
#endif
}
