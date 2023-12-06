using Minecraft.Input;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minecraft
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField]
        private Transform orientation;

        [SerializeField]
        private Animator animator;

        [field: SerializeField]
        public Rigidbody Rigidbody { get; private set; }

        [SerializeField]
        private PlayerData_SO data;

        [SerializeField]
        private float blendSpeedChangeRate;

        private float _blendSpeedValue;
        private Vector2 _moveInput;

        private void OnEnable()
        {
            MInput.Move.performed += ProcessMoveInput;
            MInput.Move.canceled += ProcessMoveInput;

            MInput.Sprint.performed += ProcessSprintInput;
            MInput.Crounch.performed += ProcessCronchInput;
            MInput.Crounch.canceled += ProcessCronchInput;
        }

        private void OnDisable()
        {
            MInput.Move.performed -= ProcessMoveInput;
            MInput.Move.canceled -= ProcessMoveInput;

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
            WaterCheck();
            ProcessJumpInput();
            ResetSprintState();
            BlendAnimation();
        }

        private void FixedUpdate()
        {
            ApplyVelocityDrag();
            Move();
            SpeedControl();
            ApplyWaterPush();
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
        private void ProcessJumpInput()
        {
            if (!MInput.Jump.IsPressed())
                return;

            if (!(data.isGrounded || data.isBobyInWater) || Time.time < data.allowJumpTime)
                return;

            data.allowJumpTime = Time.time + data.JumpCooldown;
            Jump();
        }

        private void ProcessCronchInput(InputAction.CallbackContext context)
        {
            data.isCrounching = context.performed;
            data.isSprinting &= data.isCrounching;
        }

        private void Move()
        {
            var moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
            if (moveDirection == Vector3.zero)
            {
                data.currentMoveSpeed = 0f;
                return;
            }
            float airMultilier = !data.isGrounded && data.isBobyInWater ? 10f * data.AirMultilier : 10f;
            float sprintMultilier = data.isSprinting ? data.SprintMultilier : 1f;
            float crounchMultilier = data.isCrounching && data.isGrounded ? data.CrounchMultilier : 1f;
            data.currentMoveSpeed = data.WalkSpeed * sprintMultilier * crounchMultilier;
            Rigidbody.AddForce(airMultilier * data.currentMoveSpeed * moveDirection, ForceMode.Force);
        }

        private void BlendAnimation()
        {
            _blendSpeedValue = math.lerp(_blendSpeedValue, data.currentMoveSpeed, blendSpeedChangeRate * Time.deltaTime);
            animator.SetFloat(AnimID.Speed, _blendSpeedValue);
        }

        private void ResetSprintState()
        {
            data.isSprinting = data.isSprinting && _moveInput != Vector2.zero;
        }

        private void ApplyVelocityDrag()
        {
            Rigidbody.drag = math.select(math.select(data.AirDrag, data.GroundDrag, data.isGrounded), data.WaterDrag, data.isStepInWater);
        }

        private void GroundCheck()
        {
            var spherePosition = Rigidbody.position.Add(y: data.GroundOffset);
            data.isGrounded = Physics.CheckSphere(spherePosition, data.GroundRadius, data.GroundLayer, QueryTriggerInteraction.Ignore);
        }

        private void WaterCheck()
        {
            var footPosition = Vector3Int.FloorToInt(transform.position);
            var bodyPosition = Vector3Int.FloorToInt(transform.position + data.BodyOffset);
            data.isStepInWater = Chunk.GetBlock(footPosition) == BlockType.Water;
            data.isBobyInWater = Chunk.GetBlock(bodyPosition) == BlockType.Water;
        }

        private void ApplyWaterPush()
        {
            if(!data.isBobyInWater)
                return;

            var waterForce = data.WaterPushForce * transform.up;
            Rigidbody.AddForce(waterForce, ForceMode.Force);
        }

        private void Jump()
        {
            float jumpMultilier = data.isBobyInWater ? 0.5f : 1f;
            jumpMultilier = data.isStepInWater ? 1.5f : jumpMultilier;
            Rigidbody.velocity = Rigidbody.velocity.With(y: 0);
            Rigidbody.AddForce(jumpMultilier * data.JumpForce * transform.up, ForceMode.Impulse);
        }

        private void SpeedControl()
        {
            var flatVelocity = Rigidbody.velocity.With(y: 0);
            var flatSpeed = flatVelocity.magnitude;
            if (flatSpeed <= data.currentMoveSpeed)
                return;

            var dragForce = (flatSpeed - data.currentMoveSpeed) * 2f * -flatVelocity;
            Rigidbody.AddForce(dragForce, ForceMode.Force);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = data.isGrounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(
                transform.position.Add(y: data.GroundOffset),
                data.GroundRadius);
        }
#endif
    }
}