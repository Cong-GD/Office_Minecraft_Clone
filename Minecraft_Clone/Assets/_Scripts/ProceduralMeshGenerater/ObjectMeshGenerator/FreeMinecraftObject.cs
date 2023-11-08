using NaughtyAttributes;
using ObjectPooling;
using System;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public class FreeMinecraftObject : PoolObject
    {
        [SerializeField]
        private Vector3 objectOffset;

        [SerializeField]
        private float rotateSpeed;

        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private MinecraftObjectRenderer minecraftObject;

        private readonly ItemSlot itemHolder = new();

        private float _allowTriggerTime;

        private void OnEnable()
        {
            itemHolder.OnItemModified += UpdateRenderer;
            UpdateRenderer();
        }

        private void OnDisable()
        {
            itemHolder.OnItemModified -= UpdateRenderer;
        }


        private void FixedUpdate()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.fixedDeltaTime);
        }

        public void Init(ItemPacked item, Vector3 position, Vector3 pushForce)
        {
            _allowTriggerTime = Time.time + 0.5f;
            transform.position = position;
            body.AddForce(pushForce, ForceMode.Impulse);
            itemHolder.SetItem(item);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time < _allowTriggerTime)
                return;

            if(other.gameObject.CompareTag("Player"))
            {
                InventorySystem.Instance.AddItemToInventory(itemHolder);
                if (itemHolder.IsEmpty())
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