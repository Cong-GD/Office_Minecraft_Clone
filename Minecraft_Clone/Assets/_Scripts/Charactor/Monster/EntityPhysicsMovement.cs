using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Minecraft
{
    public class EntityPhysicsMovement : MonoBehaviour
    {
        [SerializeField]
        private float gravity = -9.8f;

        [SerializeField]
        private float waterPushForce = 2f;

        [SerializeField]
        private float waterPower = 2f;

        [SerializeField]
        private CharacterController _controller;

        [SerializeField]
        private float stepOffset = 1f;

        [SerializeField, Min(0f)]
        private float moveSpeed = 5f;

        [SerializeField]
        private float waterDrag = 0.5f;

        [SerializeField]
        private float bodyPositionOffset = 1.1f;

        public bool IsMoving => _moveDirection.sqrMagnitude > 0;

        public Vector3 MoveDirection
        {
            get => _moveDirection;
            set => _moveDirection = value;
        }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public float StepOffset
        {
            get => stepOffset;
            set
            {
                stepOffset = Mathf.Max(0f, value);
                _controller.stepOffset = stepOffset;
            }
        }

        public bool IsStepingOnWater { get; private set; }

        public bool IsInWater { get; private set; }

        public Vector3 Velocity => _velocity;

        public bool IsGrounded => _controller.isGrounded;


        private Vector3 _velocity;
        private Vector3 _moveDirection;

        private void Reset()
        {
            if(_controller != null)
            {
                _controller = GetComponent<CharacterController>();
            }
        }

        private void OnValidate()
        {
            _controller.stepOffset = stepOffset;
        }

        private void Update()
        {
            WaterCheck();
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            Move();
            _controller.Move(_velocity * Time.fixedDeltaTime);
        }

        private void Move()
        {
            float drag = IsStepingOnWater ? waterDrag : 1f; 
            Vector3 moveDirection = _moveDirection.normalized;
            Vector3 moveVelocity = drag * moveSpeed * moveDirection;
            _velocity.x = moveVelocity.x;
            _velocity.z = moveVelocity.z;
        }

        private void ApplyGravity()
        {
            if (IsInWater)
            {
                _velocity.y = math.lerp(_velocity.y, waterPushForce, waterPower * Time.fixedDeltaTime);
                return;
            }

            if(IsGrounded)
            {
                _velocity.y = -1f;
                return;
            }
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        private void WaterCheck()
        {
            IsInWater = Chunk.CheckWater(transform.position.Add(y: bodyPositionOffset));
            IsStepingOnWater = Chunk.CheckWater(transform.position);
        }
    }
}
