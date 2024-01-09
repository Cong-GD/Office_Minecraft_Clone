using CongTDev.Collection;
using NaughtyAttributes;
using ObjectPooling;
using System.Collections.Generic;
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

        private List<FreeMinecraftObject> _activefreeObjects = new();
        private Stack<FreeMinecraftObject> _returningObject = new();
        private Vector3 _playerPosition;
        private ByteString dropItemData;

        [ShowNativeProperty]
        public int ActiveCount => _activefreeObjects.Count;

        protected override void Awake()
        {
            base.Awake();
            GameManager.Instance.OnGameSave += OnGameSave;
            GameManager.Instance.OnGameLoad += OnGameLoad;
            World.Instance.OnWorldLoaded += LoadDropItems;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameSave -= OnGameSave;
            GameManager.Instance.OnGameLoad -= OnGameLoad;
            if (World.Instance)
            {
                World.Instance.OnWorldLoaded -= LoadDropItems;
            }
        }

        private void FixedUpdate()
        {
            foreach (FreeMinecraftObject freeObject in _activefreeObjects)
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

            while (_returningObject.TryPop(out FreeMinecraftObject freeObject))
            {
                _activefreeObjects.Remove(freeObject);
                freeObject.ReturnToPool();
            }
        }

        private void OnGameSave(Dictionary<string, ByteString> dictionary)
        {
            ByteString byteString = ByteString.Create(_activefreeObjects.Count * 25);
            byteString.WriteValue(_activefreeObjects.Count);
            foreach (FreeMinecraftObject freeObject in _activefreeObjects)
            {
                freeObject.GetItemHolding().WriteTo(byteString);
                byteString.WriteValue(freeObject.Rigidbody.position);
            }
            dictionary["DropItems.dat"] = byteString;
        }

        private void OnGameLoad(Dictionary<string, ByteString> dictionary)
        {
            dictionary.Remove("DropItems.dat", out dropItemData);
        }

        private void LoadDropItems()
        {
            if (dropItemData == null)
                return;

            ByteString.BytesReader byteReader = dropItemData.GetBytesReader();
            int count = byteReader.ReadValue<int>();
            for (int i = 0; i < count; i++)
            {
                ItemPacked item = ItemPacked.ParseFrom(ref byteReader);
                Vector3 position = byteReader.ReadValue<Vector3>();
                ThrowItem(item, position, Vector3.zero);
            }
            dropItemData.Dispose();
            dropItemData = null;
        }

        public void ThrowItem(ItemPacked item, Vector3 position, Vector3 force)
        {
            if (item.IsEmpty())
                return;

            FreeMinecraftObject instance = (FreeMinecraftObject)freeObjectPool.Get(transform);
            _activefreeObjects.Add(instance);
            instance.Init(item, position, force);
        }

        private bool ItemLifeTimePass(FreeMinecraftObject freeObject)
        {
            if (Time.time > freeObject.ActivatedTime + itemLifeTime)
            {
                _returningObject.Push(freeObject);
                return true;
            }
            return false;
        }

        private bool MaxDistancePass(FreeMinecraftObject freeObject, float distanceToPlayer)
        {
            if (distanceToPlayer > maxDistanceFormPlayer)
            {
                _returningObject.Push(freeObject);
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
                _returningObject.Push(freeObject);
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