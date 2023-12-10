using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public class SampleAI : MonoBehaviour, ISearcher
    {
        [SerializeField]
        private EntityPhysicsMovement movement;

        [SerializeField]
        private Transform target;

        [SerializeField]
        private PathFinding pathFinding;


        public Vector3[] path;

        private int pathIndex;

        [SerializeField]
        private float endNodeDistance = 0.1f;

        private Coroutine coroutine;

        [Button]
        public void FindPlayer()
        {
            pathFinding.FindPathAsync(this, transform.position + Vector3.up, target.position + Vector3.up);
        }

        private IEnumerator FollowPath()
        {
            while (pathIndex < path.Length)
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
                Gizmos.color = Color.blue;
                for (int i = pathIndex; i < path.Length; i++)
                {
                    Gizmos.DrawSphere(path[i], 0.2f);
                }
            }
        }

        public bool CanTraverse(NodeView source, NodeView dest)
        {
            if(dest.BlockData.IsSolid)
                return false;

            (int destX, int destY, int destZ) = dest.Position;

            BlockData_SO blockAtHead = Chunk.GetBlock(destX, destY + 1, destZ).Data();
            if(blockAtHead.IsSolid)
                return false;

            (int srcX, int srcY, int srcZ) = source.Position;

            bool isStepingOnGround = Chunk.GetBlock(srcX, srcY - 1, srcZ).Data().IsSolid;
            if(!isStepingOnGround)
            {
                bool isFallingDown = srcX == destX && srcZ == destZ && destY == srcY - 1;
                return isFallingDown;
            }

            bool hasBlockBelowFoot = Chunk.GetBlock(destX, destY - 1, destZ).Data().IsSolid;
            if(!hasBlockBelowFoot)
            {
                bool isStepDown = destY == srcY - 1;
                return isStepDown;
            }
            return true;
        }

        public void OnPathFound(Vector3[] path)
        {
            if(Thread.CurrentThread.ManagedThreadId != 1)
            {
                Debug.LogError("OnPathFound is not called on main thread");
            }

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            this.path = path;
            pathIndex = 0;
            coroutine = StartCoroutine(FollowPath());
        }
    }
}
