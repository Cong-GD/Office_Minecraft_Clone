using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public class SampleAI : MonoBehaviour
    {
        [SerializeField]
        private EntityPhysicsMovement _movement;

        [SerializeField]
        private Transform _target;

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 direction = (_target.position - transform.position).With(y: 0);
            direction.Normalize();
            _movement.MoveDirectionInput = direction;
        }
    }
}
