using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public class SampleAI : MonoBehaviour
    {
        [SerializeField]
        private EntityPhysicsMovement movement;

        [SerializeField]
        private Transform target;

        [SerializeField]
        private PathFinding pathFinding;


        public List<Vector3> path;

        private int pathIndex;

        [SerializeField]
        private float endNodeDistance = 0.1f;

        [Button]
        public IEnumerator StartFindPlayer()
        {
            path = pathFinding.FindPath(transform.position, target.position + Vector3.up);
            pathIndex = 0;

            while (pathIndex < path.Count)
            {
                Vector3 targetPosition = path[pathIndex];
                Vector3 direction = (targetPosition - transform.position).With(y: 0);

                movement.MoveDirectionInput = direction;

                if (Vector3.Distance(transform.position, targetPosition) < endNodeDistance)
                {
                    pathIndex++;
                }

                yield return null;
            }
            movement.MoveDirectionInput = Vector3.zero;
        }

        [Button]
        public void TeleportToPlayer()
        {
            transform.position = target.position;
        }

        private void OnDrawGizmos()
        {
            if(path != null)
            {
                Gizmos.color = Color.red;
                for (int i = pathIndex; i < path.Count; i++)
                {
                    Gizmos.DrawSphere(path[i], 0.1f);
                }
            }
        }


    }
}
