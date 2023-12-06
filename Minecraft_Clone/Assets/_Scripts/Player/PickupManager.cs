using NaughtyAttributes;
using ObjectPooling;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft
{
    public class PickupManager : GlobalReference<PickupManager>
    {
        [SerializeField]
        private ObjectPool freeObjectPool;

        [SerializeField]
        private float pickupAllowTime = 0.5f;

        [SerializeField]
        private float objectRotateSpeed;

        [SerializeField]
        private float itemSuckRange;

        [SerializeField]
        private float itemSuckSpeed;

        [SerializeField]
        private float pickupRange;

        [SerializeField]
        private float itemLifeTime;

        [SerializeField]
        private float maxDistanceFormPlayer;

        [SerializeField]
        private Rigidbody playerBody;

        private HashSet<FreeMinecraftObject> _activefreeObjects = new();

        private Queue<FreeMinecraftObject> _returningObject = new();

        private Vector3 _playerPosition;

        [ShowNativeProperty]
        public int ActiveCount => _activefreeObjects.Count;

        private void FixedUpdate()
        {
            foreach (var freeObject in _activefreeObjects)
            {
                if (ItemLifeTimePass(freeObject))
                    continue;

                _playerPosition = playerBody.position.Add(y: 1.5f);
                float distanceToPlayer = Vector3.Distance(freeObject.transform.position, _playerPosition);

                if (MaxDistancePass(freeObject, distanceToPlayer))
                    continue;

                if (ItemSuckPass(freeObject, distanceToPlayer))
                    continue;

                if (ItemPickupPass(freeObject, distanceToPlayer))
                    continue;

                freeObject.Rotate(objectRotateSpeed * Time.fixedDeltaTime);
            }

            while (_returningObject.TryDequeue(out var freeObject))
            {
                _activefreeObjects.Remove(freeObject);
                freeObject.ReturnToPool();
            }
        }

        public void ThrowItem(ItemPacked item, Vector3 position, Vector3 force)
        {
            if (item.IsEmpty())
                return;

            var instance = (FreeMinecraftObject)freeObjectPool.Get();
            _activefreeObjects.Add(instance);
            instance.Init(item, position, force);
        }

        private bool ItemLifeTimePass(FreeMinecraftObject freeObject)
        {
            if (Time.time > freeObject.ActivatedTime + itemLifeTime)
            {
                _returningObject.Enqueue(freeObject);
                return true;
            }
            return false;
        }

        private bool MaxDistancePass(FreeMinecraftObject freeObject, float distanceToPlayer)
        {
            if (distanceToPlayer > maxDistanceFormPlayer)
            {
                _returningObject.Enqueue(freeObject);
                return true;
            }
            return false;
        }

        private bool ItemSuckPass(FreeMinecraftObject freeObject, float distanceToPlayer)
        {
            if (distanceToPlayer < itemSuckRange && IsPickUpAble(freeObject))
            {
                var direction = (_playerPosition - freeObject.transform.position).normalized;
                freeObject.MovePosition(itemSuckSpeed * Time.fixedDeltaTime * direction);
            }
            return false;
        }

        private bool ItemPickupPass(FreeMinecraftObject freeObject, float distanceToPlayer)
        {
            if (distanceToPlayer < pickupRange && IsPickUpAble(freeObject) && freeObject.AddToIventory())
            {
                _returningObject.Enqueue(freeObject);
                return true;
            }
            return false;
        }

        private bool IsPickUpAble(FreeMinecraftObject freeObject)
        {
            return Time.time > freeObject.ActivatedTime + pickupAllowTime;
        }
    }
}