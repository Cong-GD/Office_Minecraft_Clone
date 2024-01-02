using Unity.Mathematics;
using UnityEngine;

namespace Minecraft
{
    public class EntityPhysicsMovement : MonoBehaviour, IPushAble
    {
        [SerializeField]
        private float gravity = -9.8f;

        [SerializeField]
        private float waterPushForce = 2f;

        [SerializeField]
        private float waterPower = 2f;

        [SerializeField]
        private CharacterController controller;

        [SerializeField]
        private float stepOffset = 1f;

        [SerializeField, Min(0f)]
        private float moveSpeed = 5f;

        [SerializeField]
        private float waterDrag = 0.5f;

        [SerializeField]
        private float bodyPositionOffset = 1.1f;

        [SerializeField]
        private float pushResistance = 10f;

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
                controller.stepOffset = stepOffset;
            }
        }

        public float PushResistance
        {
            get => pushResistance;
            set => pushResistance = Mathf.Max(0f, value);
        }

        public bool IsStepingOnWater { get; private set; }

        public bool IsInWater { get; private set; }

        public Vector3 Velocity => _velocity;

        public bool IsGrounded => controller.isGrounded;


        private Vector3 _velocity;
        private Vector3 _moveDirection;
        private Vector3 _outerPushForce;

        private void Reset()
        {
            if (controller != null)
            {
                controller = GetComponent<CharacterController>();
            }
        }

        private void OnValidate()
        {
            if (controller == null)
                TryGetComponent(out controller);

            if (controller == null)
                return;

            controller.stepOffset = stepOffset;
        }

        private void Update()
        {
            WaterCheck();
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            Move();
            ApplyPushForce();
            controller.Move(_velocity * Time.fixedDeltaTime);
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
                _velocity.y = Mathf.Lerp(_velocity.y, waterPushForce, waterPower * Time.fixedDeltaTime);
                return;
            }

            if (IsGrounded)
            {
                _velocity.y = Mathf.Lerp(_velocity.y, -1f, Time.fixedDeltaTime);
                return;
            }
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        private void WaterCheck()
        {
            Vector3 position = transform.position;
            IsStepingOnWater = Chunk.CheckWater(position);
            IsInWater = Chunk.CheckWater(position.Add(y: bodyPositionOffset));
        }

        private void ApplyPushForce()
        {
            if (_outerPushForce.sqrMagnitude < 0.1f)
            {
                _outerPushForce = Vector3.zero;
                return;
            }

            _outerPushForce = Vector3.Lerp(_outerPushForce, Vector3.zero, pushResistance * Time.fixedDeltaTime);
            _velocity += _outerPushForce;
        }

        public void Push(Vector3 pushForce)
        {
            _outerPushForce += pushForce;
        }
    }
}
