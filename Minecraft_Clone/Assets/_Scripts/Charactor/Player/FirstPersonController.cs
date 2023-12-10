using Minecraft.Input;
using System;
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
        private PlayerData_SO playerData;

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
            playerData.ClearTempData();
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
            if (playerData.isCrounching)
                return;

            playerData.isSprinting = true;
        }

        private void ProcessMoveInput(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void ProcessJumpInput()
        {
            if (!MInput.Jump.IsPressed())
                return;

            if (!(playerData.isGrounded || playerData.isBobyInWater) || Time.time < playerData.allowJumpTime)
                return;

            playerData.allowJumpTime = Time.time + playerData.JumpCooldown;
            Jump();
        }

        private void ProcessCronchInput(InputAction.CallbackContext context)
        {
            playerData.isCrounching = context.performed;
            playerData.isSprinting &= playerData.isCrounching;
        }

        private void Move()
        {
            var moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
            if (moveDirection == Vector3.zero)
            {
                playerData.currentMoveSpeed = 0f;
                return;
            }
            float airMultilier = !playerData.isGrounded && !playerData.isBobyInWater ? 10f * playerData.AirMultilier : 10f;
            float sprintMultilier = playerData.isSprinting ? playerData.SprintMultilier : 1f;
            float crounchMultilier = playerData.isCrounching && playerData.isGrounded ? playerData.CrounchMultilier : 1f;
            playerData.currentMoveSpeed = playerData.WalkSpeed * sprintMultilier * crounchMultilier;
            Rigidbody.AddForce(airMultilier * playerData.currentMoveSpeed * moveDirection, ForceMode.Force);
        }

        private void BlendAnimation()
        {
            _blendSpeedValue = math.lerp(_blendSpeedValue, playerData.currentMoveSpeed, blendSpeedChangeRate * Time.deltaTime);
            animator.SetFloat(AnimID.Speed, _blendSpeedValue);
        }

        private void ResetSprintState()
        {
            playerData.isSprinting = playerData.isSprinting && _moveInput != Vector2.zero;
        }

        private void ApplyVelocityDrag()
        {
            float dragValue = playerData.isGrounded ? playerData.GroundDrag : playerData.AirDrag;
            dragValue = playerData.isStepInWater ? playerData.WaterDrag : dragValue;
            Rigidbody.drag = dragValue;
        }

        private void GroundCheck()
        {
            
            var spherePosition = Rigidbody.position.Add(y: playerData.GroundOffset);
            playerData.isGrounded = Physics.CheckSphere(spherePosition, playerData.GroundRadius, playerData.GroundLayer, QueryTriggerInteraction.Ignore);
        }

        private void WaterCheck()
        {
            var footPosition = Vector3Int.FloorToInt(transform.position);
            var bodyPosition = Vector3Int.FloorToInt(transform.position + playerData.BodyOffset);
            playerData.isStepInWater = Chunk.GetBlock(footPosition) == BlockType.Water;
            playerData.isBobyInWater = Chunk.GetBlock(bodyPosition) == BlockType.Water;
        }

        private void ApplyWaterPush()
        {
            if (!playerData.isBobyInWater)
                return;

            var waterForce = playerData.WaterPushForce * Vector3.up;
            Rigidbody.AddForce(waterForce, ForceMode.Force);
        }

        private void Jump()
        {
            float jumpMultilier = 1f;
            if (playerData.isStepInWater)
            {
                jumpMultilier = 0.5f;
                jumpMultilier += HasGroundInFront() ? playerData.HelpForceToLeaveWater : 0f;
            }

            Rigidbody.velocity = Rigidbody.velocity.With(y: 0);
            Rigidbody.AddForce(jumpMultilier * playerData.JumpForce * Vector3.up, ForceMode.Impulse);
        }

        private bool HasGroundInFront()
        {
            var frontPosition = orientation.position + orientation.forward;
            var block = Chunk.GetBlock(frontPosition);
            return block.Data().IsSolid;
        }

        private void SpeedControl()
        {
            var flatVelocity = Rigidbody.velocity.With(y: 0);
            var flatSpeed = flatVelocity.magnitude;
            if (flatSpeed <= playerData.currentMoveSpeed)
                return;

            var dragForce = (flatSpeed - playerData.currentMoveSpeed) * 2f * -flatVelocity;
            Rigidbody.AddForce(dragForce, ForceMode.Force);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = playerData.isGrounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(
                transform.position.Add(y: playerData.GroundOffset),
                playerData.GroundRadius);
        }
#endif
    }
}