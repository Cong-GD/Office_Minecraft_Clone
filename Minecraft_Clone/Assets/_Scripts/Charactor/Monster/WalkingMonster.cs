using CongTDev.Collection;
using FMODUnity;
using Minecraft.Audio;
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

namespace Minecraft.AI
{

    public class WalkingMonster : BaseMonster, ISearcher
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

        [SerializeField]
        private Transform attackRoot;

        [SerializeField]
        private float attackRange = 1f;

        [SerializeField]
        private float knockBackForce = 5f;

        [SerializeField]
        private LayerMask playerCombatLayer;

        [SerializeField]
        private Vector3 rayCastStartOffset;

        [SerializeField]
        private int damage = 3;

        [SerializeField]
        private float attackInterval = 1f;


        private MyNativeList<Vector3> _path = new MyNativeList<Vector3>();
        private int _pathIndex;
        private VoxelSearchContext.CancelToken _searchToken;
        private bool _isChasing;
        private float _lastAttackTime;


        private void Update()
        {
            if(_isChasing && Time.time > _lastAttackTime + attackInterval)
            {
                
                Ray ray = new Ray(attackRoot.position + rayCastStartOffset, attackRoot.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, attackRange, playerCombatLayer))
                {
                    if (hit.collider.TryGetComponent(out Health health))
                    {
                        health.TakeDamage(damage, DamegeType.Physic);
                        playerData.PlayerBody.AddForce(attackRoot.forward.Add(y: 0.1f) * knockBackForce, ForceMode.Impulse);
                        _lastAttackTime = Time.time;
                    }
                }
            }

            if (_pathIndex < _path.Count)
            {
                Vector3 direction = _path[_pathIndex] - transform.position;
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
                if (Vector3.Distance(transform.position, playerData.PlayerBody.position) < detectRange)
                {
                    yield return ChasingState();
                }
                else
                {
                    //yield return PatrolState();
                }
                yield return Wait.ForSeconds(0.5f);
            }
        }

        private IEnumerator ChasingState()
        {
            while (true)
            {
                _isChasing = true;
                if (Vector3.Distance(transform.position, playerData.PlayerBody.position) > detectRange * 2f)
                {
                    _isChasing = false;
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
                float randomX = transform.position.x + UnityEngine.Random.Range(-10f, 10f);
                float randomZ = transform.position.z + UnityEngine.Random.Range(-10f, 10f);
                Vector3Int randomPosition = new Vector3Int((int)randomX, (int)transform.position.y, (int)randomZ);
                for (int i = -5; i < 10; i++)
                {

                }

                yield return Wait.ForSeconds(1f);
            }
        }

        public override void SetPosition(Vector3 position)
        {
            movement.SetPosition(position);
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

            // if there is no ground below the dest, it will fall down
            bool isThereWillBeGround = context.GetNode(destX, destY - 1, destZ).BlockData.IsSolid;
            if (!isThereWillBeGround)
            {
                bool isStepdown = destY == srcY - 1;
                return isStepdown && !context.GetNode(destX, destY + 1, destZ).BlockData.IsSolid;
            }

            if (srcX != destX && srcZ != destZ)
            {
                if (destY != srcY)
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

            return true;
        }

        public void OnPathFound(VoxelSearchContext.SearchResult searchResult)
        {
            searchResult.GetPath(_path);
            _pathIndex = 0;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_path.Count == 0)
                return;

            Gizmos.color = Color.red;
            Vector3 startPosition = attackRoot.position + rayCastStartOffset;
            Gizmos.DrawSphere(startPosition, 0.05f);
            Gizmos.DrawSphere(startPosition + attackRoot.forward * attackRange, 0.05f);

            Gizmos.color = Color.blue;
            for (int i = _pathIndex; i < _path.Count; i++)
            {
                Gizmos.DrawSphere(_path[i], 0.2f);
            }
        }
#endif

        public override ByteString ToByteString()
        {
            ByteString byteString = ByteString.Create();
            byteString.WriteValue(transform.position);
            byteString.WriteValue(Health.CurrentHealth);
            return byteString;
        }

        public override void FromByteString(ByteString byteString)
        {
            ByteString.BytesReader byteReader = byteString.GetBytesReader();
            movement.SetPosition(byteReader.ReadValue<Vector3>());
            Health.CurrentHealth = byteReader.ReadValue<int>();
        }
    }
}
