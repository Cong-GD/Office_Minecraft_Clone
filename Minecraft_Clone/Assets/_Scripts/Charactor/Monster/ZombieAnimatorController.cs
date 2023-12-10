using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Minecraft
{
    public class ZombieAnimatorController : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private EntityPhysicsMovement _movement;

        [SerializeField]
        private Transform _modelRoot;

        [SerializeField]
        private float _turnSpeed = 0.1f;

        [SerializeField]
        private float _speedBlendSpeed = 10f;

        private float _speedBlendValue;

        private void Update()
        {
            if(_movement.IsMoving)
            {
                var targetRotation = Quaternion.LookRotation(_movement.MoveDirectionInput.With(y: 0));
                _modelRoot.rotation = Quaternion.RotateTowards(_modelRoot.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
            var speed = _movement.Velocity.XZ().magnitude;
            _speedBlendValue = Mathf.Lerp(_speedBlendValue, speed, Time.deltaTime * _speedBlendSpeed);

            _animator.SetFloat(AnimID.Speed, _speedBlendValue);
        }
    }
}
