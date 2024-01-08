using CongTDev.Collection;
using NaughtyAttributes;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Minecraft.AI
{
    public class WalkingMonster : MonoBehaviour, ISearcher
    {
        [SerializeField]
        private EntityPhysicsMovement movement;

        [SerializeField]
        private PlayerData_SO playerData;

        [SerializeField]
        private PathFinding pathFinding;

        [SerializeField]
        private float endNodeDistance = 1f;

        [SerializeField]
        private float detectRange = 10f;

        [ShowNativeProperty]
        public int PathLength => _path.Count;

        private MyNativeList<Vector3> _path = new MyNativeList<Vector3>();
        private int _pathIndex;
        private VoxelSearchContext.CancelToken _searchToken;


        private void Update()
        {
            if (_pathIndex < _path.Count)
            {
                Vector3 targetPosition = _path[_pathIndex];
                Vector3 direction = targetPosition - transform.position;

                movement.MoveDirection = direction.With(y: 0);

                if (direction.magnitude < endNodeDistance)
                {
                    _pathIndex++;
                }
            }
            else
            {
                movement.MoveDirection = Vector3.zero;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(Routine());
        }

        private void OnDisable()
        {
            _searchToken.Cancel();
        }

        private IEnumerator Routine()
        {
            while (true)
            {
                if(Vector3.Distance(transform.position, playerData.PlayerBody.position) < detectRange)
                {
                    yield return ChasingState();
                }
                else
                {
                    yield return PatrolState();
                }
            }
        }

        private IEnumerator ChasingState()
        {
            while(true)
            {
                if (Vector3.Distance(transform.position, playerData.PlayerBody.position) > detectRange)
                {
                    yield break;
                }

                FindPathTo(playerData.PlayerBody.position.Add(0.5f));
                yield return Wait.ForSeconds(1f);
            }
        }

        private IEnumerator PatrolState()
        {
            while (true)
            {
                if (Vector3.Distance(transform.position, playerData.PlayerBody.position) < detectRange)
                {
                    yield break;
                }


                yield return Wait.ForSeconds(5f);
            }
        }

        public void FindPathTo(Vector3 position)
        {
            _searchToken.Cancel();
            _searchToken = pathFinding.FindPathAsync(
                searcher: this, 
                start: transform.position.Add(y: 0.2f), 
                end: position, 
                flattenY: true);
        }

        public bool CanTraverse(VoxelSearchContext.NodeProvider context, NodeView source, NodeView dest)
        {
            if (dest.BlockData.IsSolid)
                return false;

            (int destX, int destY, int destZ) = dest.Position;
            (int srcX, int srcY, int srcZ) = source.Position;

            // if there is a solid block above the dest, it can not go there
            BlockData_SO blockForhead = context.GetNode(destX, destY + 1, destZ).BlockData;
            if (blockForhead.IsSolid)
                return false;


            bool isInWater = source.BlockData.BlockType == BlockType.Water;
            if (isInWater)
            {
                bool isSinked = context.GetNode(srcX, srcY + 1, srcZ).BlockData.BlockType == BlockType.Water;
                if (isSinked)
                {
                    bool isSwimmingUp = srcX == destX && srcZ == destZ && destY == srcY + 1;
                    return isSwimmingUp;
                }

                bool isGoingToWater = dest.BlockData.BlockType == BlockType.Water;
                if (isGoingToWater)
                {
                    bool isNotSwimmingDown = destY >= srcY;
                    return isNotSwimmingDown;
                }
            }

            // if it is not on the ground, it is falling down
            bool isSteppingOnGround = context.GetNode(srcX, srcY - 1, srcZ).BlockData.IsSolid;
            if (!isInWater && !isSteppingOnGround)
            {
                bool isFallingDown = srcX == destX && srcZ == destZ && destY == srcY - 1;
                return isFallingDown;
            }

            if (srcX != destX && srcZ != destZ)
            {
                if(destY != srcY)
                {
                    return false;
                }
                if (context.GetNode(srcX + (destX - srcX), destY, srcZ).BlockData.IsSolid
                    || context.GetNode(srcX, destY, srcZ + (destZ - srcZ)).BlockData.IsSolid
                    || context.GetNode(srcX + (destX - srcX), destY + 1, srcZ).BlockData.IsSolid
                    || context.GetNode(srcX, destY + 1, srcZ + (destZ - srcZ)).BlockData.IsSolid)
                {
                    return false;
                }
            }

            // if there is no ground below the dest, it will fall down
            bool isThereWillBeGround = context.GetNode(destX, destY - 1, destZ).BlockData.IsSolid;
            if (!isThereWillBeGround)
            {
                bool isStepdown = destY == srcY - 1;
                return isStepdown && context.GetNode(destX, destY + 1, destZ).BlockData.IsSolid;
            }

            return true;
        }

        public void OnPathFound(VoxelSearchContext.SearchResult searchResult)
        {
            searchResult.GetPath(_path);
            _pathIndex = 0;
        }

        private void OnDrawGizmos()
        {
            if (_path.Count == 0)
                return;

            Gizmos.color = Color.blue;
            for (int i = _pathIndex; i < _path.Count; i++)
            {
                Gizmos.DrawSphere(_path[i], 0.2f);
            }
        }
    }
}
