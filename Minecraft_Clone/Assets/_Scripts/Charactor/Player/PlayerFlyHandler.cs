using Minecraft.Input;
using System;
using System.Collections;
using UnityEngine;

namespace Minecraft.Assets._Scripts.Charactor.Player
{
    public class PlayerFlyHandler : MonoBehaviour
    {
        [SerializeField]
        private PlayerData_SO playerData;

        [SerializeField]
        private PlayerController playerController;

        [SerializeField]
        private Transform orientation;

        [SerializeField]
        private float exitFlyTimeThreshold = 0.8f;

        [SerializeField]
        private float initPushForce = 10f;

        private Rigidbody _body;
        private float _groundedTimer;

        private void Awake()
        {
            _body = playerData.PlayerBody;
        }

        private void OnEnable()
        {
            _body.useGravity = false;
            _body.velocity = Vector3.up * initPushForce;
            _body.drag = playerData.FlyingDragForce;
        }

        private void OnDisable()
        {
            _body.useGravity = true;
        }

        private void Update()
        {
            if (playerData.isGrounded)
            {
                _groundedTimer += Time.deltaTime;
                if (_groundedTimer >= exitFlyTimeThreshold)
                {
                    playerController.ExitFlyMode();
                    _groundedTimer = 0;
                }
            }
            else
            {
                _groundedTimer = 0;
            }
        }

        private void FixedUpdate()
        {
            Fly();
            FlyUpDown();
            SpeedControl();
        }

        private void Fly()
        {
            Vector2 input = MInput.Move.ReadValue<Vector2>();
            if(input == Vector2.zero)
            {
                playerData.currentMoveSpeed = 0;
                return;
            }

            Vector3 moveDirection = orientation.forward * input.y + orientation.right * input.x;
            playerData.currentMoveSpeed = playerData.isSprinting ? playerData.FlySprintSpeed : playerData.FlySpeed;
            _body.AddForce(playerData.currentMoveSpeed * moveDirection, ForceMode.VelocityChange);
        }

        private void FlyUpDown()
        {
            if (MInput.Crounch.IsPressed())
            {
                _body.AddForce(Vector3.down * playerData.FlyDownForce, ForceMode.VelocityChange);
            }
            else if (MInput.Jump.IsPressed())
            {
                _body.AddForce(Vector3.up * playerData.FlyUpForce, ForceMode.VelocityChange);
            }
        }

        private void SpeedControl()
        {
            Vector3 velocity = _body.velocity;
            float velocityY = velocity.y;
            velocity = Vector3.ClampMagnitude(velocity.With(y: 0), playerData.currentMoveSpeed);
            _body.velocity = velocity.With(y: velocityY);
        }
    }
}