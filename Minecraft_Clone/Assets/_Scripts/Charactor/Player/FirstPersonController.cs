using FMOD.Studio;
using FMODUnity;
using Minecraft.Audio;
using Minecraft.Input;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minecraft
{
    public class FirstPersonController : MonoBehaviour, IPushAble, IMovementData
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

        public Vector3 Velocity => Rigidbody.velocity;

        public bool IsGrounded => playerData.isGrounded;

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
            ProcessJumpInput();
            ResetSprintState();
            BlendAnimation();
        }

        private void FixedUpdate()
        {
            ApplyVelocityDrag();
            Move();
            DiveCheck();
            SpeedControl();
            ApplyWaterPush();
        }

        public void Push(Vector3 pushForce)
        {
            Rigidbody.AddForce(pushForce * 2f, ForceMode.Impulse);
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
            if(playerData.isCrounching)
            {
                playerData.isSprinting = false;
            }
        }

        private void Move()
        {
            Vector3 moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
            if (moveDirection == Vector3.zero)
            {
                playerData.currentMoveSpeed = 0f;
                return;
            }
            float airMultilier = 10f;
            if (!playerData.isGrounded && !playerData.isStepInWater && !playerData.isBobyInWater)
            {
                airMultilier *= playerData.AirMultilier;
            }

            float sprintMultilier = 1f;
            if (playerData.isSprinting)
            {
                sprintMultilier = playerData.SprintMultilier;
            }

            float crounchMultilier = 1f;
            if (playerData.isCrounching && playerData.isGrounded)
            {
                crounchMultilier = playerData.CrounchMultilier;
            }

            float waterMultilier = 1f;
            if (playerData.isStepInWater || playerData.isBobyInWater)
            {
                waterMultilier = playerData.WaterMultilier;
            }

            playerData.currentMoveSpeed = playerData.WalkSpeed * sprintMultilier * crounchMultilier * waterMultilier;

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
            if (playerData.isStepInWater || playerData.isBobyInWater)
            {
                dragValue = playerData.WaterDrag;
            }
            Rigidbody.drag = dragValue;
        }

        private void ApplyWaterPush()
        {
            if (!playerData.isBobyInWater)
                return;

            Vector3 waterForce = playerData.WaterPushForce * Vector3.up;
            Rigidbody.AddForce(waterForce, ForceMode.Force);
        }

        private void Jump()
        {
            if (playerData.isCrounching)
                return;

            if (playerData.isBobyInWater)
            {
                Rigidbody.velocity = Rigidbody.velocity.With(y: 0);
                Rigidbody.AddForce(playerData.SwinForce * Vector3.up, ForceMode.Impulse);
                return;
            }

            float jumpMultilier = 1f;
            
            if (playerData.isStepInWater)
            {
                jumpMultilier = 0.5f;
                if (HasGroundInFront())
                {
                    jumpMultilier += (float)playerData.HelpForceToLeaveWater;
                }
            }

            Rigidbody.velocity = Rigidbody.velocity.With(y: 0);
            Rigidbody.AddForce(jumpMultilier * playerData.JumpForce * Vector3.up, ForceMode.Impulse);
        }

        private bool HasGroundInFront()
        {
            Vector3 frontPosition = orientation.position + orientation.forward;
            BlockType block = Chunk.GetBlock(frontPosition);
            return block.Data().IsSolid;
        }

        private void DiveCheck()
        {
            if(playerData.isBobyInWater && playerData.isCrounching)
            {
                Rigidbody.AddForce(playerData.DiveForce * Vector3.down, ForceMode.Force);
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVelocity = Rigidbody.velocity.With(y: 0);
            float flatSpeed = flatVelocity.magnitude;
            if (flatSpeed <= playerData.currentMoveSpeed)
                return;

            Vector3 dragForce = (flatSpeed - playerData.currentMoveSpeed) * 2f * -flatVelocity;
            Rigidbody.AddForce(dragForce, ForceMode.Force);
        }
    }
}