using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Minecraft
{
    public class EntityPhysicsMovement : MonoBehaviour
    {
        [SerializeField]
        private float _gravity = -9.8f;

        [SerializeField]
        private CharacterController _controller;

        [SerializeField]
        private float stepOffset = 1f;

        [SerializeField]
        private float moveSpeed = 5f;

        [SerializeField]
        private Vector3 moveDirectionInput;

        public bool IsMoving => moveDirectionInput.sqrMagnitude > 0;

        public Vector3 MoveDirectionInput
        {
            get => moveDirectionInput;
            set => moveDirectionInput = value.normalized;
        }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0, value);
        }

        public float StepOffset
        {
            get => stepOffset;
            set => stepOffset = Mathf.Max(0, value);
        }

        public Vector3 Velocity => _velocity;

        public bool IsGrounded => _controller.isGrounded;


        private Vector3 _velocity;

        private void Reset()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Awake()
        {
            _controller.stepOffset = stepOffset;
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            Move();
            _controller.Move(_velocity * Time.fixedDeltaTime);
        }

        private void Move()
        {
            Vector3 moveDirection = moveDirectionInput.normalized;
            Vector3 moveVelocity = moveDirection * moveSpeed;
            _velocity.x = moveVelocity.x;
            _velocity.z = moveVelocity.z;
        }

        private void ApplyGravity()
        {
            if(IsGrounded)
            {
                _velocity.y = -1f;
                return;
            }
            _velocity.y += _gravity * Time.fixedDeltaTime;
        }
    }
}
