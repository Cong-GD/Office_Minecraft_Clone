using NaughtyAttributes;
using ObjectPooling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public class FreeMinecraftObject : PoolObject
    {
        private static ObjectPool _pool;

        private static HashSet<FreeMinecraftObject> _activeInstance = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _pool = Resources.Load<ObjectPool>("Pools/FreeMinecraftObjectPool");
        }

        public FreeMinecraftObject[] GetAllCurrentActiveObject() => _activeInstance.ToArray();

        public static void ThrowItem(ItemPacked item, Vector3 position, Vector3 force)
        {
            if (item.IsEmpty())
                return;

            var instance = (FreeMinecraftObject)_pool.Get();
            instance.Init(item, position, force);
        }


        [SerializeField]
        private Vector3 objectOffset;

        [SerializeField]
        private float rotateSpeed;

        [SerializeField]
        private MinecraftObjectRenderer minecraftObject;

        private readonly ItemSlot itemHolder = new();

        [field: SerializeField]
        public Rigidbody Rigidbody { get; private set; }

        public float ActivatedTime { get; private set; }

        private void OnEnable()
        {
            itemHolder.OnItemModified += UpdateRenderer;
            UpdateRenderer();
        }

        private void OnDisable()
        {
            Rigidbody.velocity = Vector3.zero;
            itemHolder.OnItemModified -= UpdateRenderer;
        }


        private void FixedUpdate()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.fixedDeltaTime);
        }

        private void Init(ItemPacked item, Vector3 position, Vector3 pushForce)
        {
            _activeInstance.Add(this);
            ActivatedTime = Time.time;
            transform.position = position;
            Rigidbody.velocity = pushForce;
            itemHolder.SetItem(item);
        }

        public void GoOff()
        {
            itemHolder.SetItem(ItemPacked.Empty);
            _activeInstance.Remove(this);
            ReturnToPool();
        }

        public void AddToIventory()
        {
            InventorySystem.Instance.AddItemToInventory(itemHolder);
            if (itemHolder.IsEmpty())
            {
                _activeInstance.Remove(this);
                ReturnToPool();
            }
        }

        private void UpdateRenderer()
        {
            if (itemHolder.IsEmpty())
            {
                minecraftObject.Clear();
                return;
            }

            minecraftObject.RenderObject(itemHolder.RootItem.GetObjectMeshData(), ItemTransformState.FreeObject);
            minecraftObject.transform.Translate(objectOffset);
        }
    }
}