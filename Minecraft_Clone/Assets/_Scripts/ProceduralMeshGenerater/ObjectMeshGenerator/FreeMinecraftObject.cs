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
        [SerializeField]
        private Vector3 objectOffset;

        private float _currentYRotation;

        [SerializeField]
        private MinecraftObjectRenderer minecraftObject;

        private readonly ItemSlot itemHolder = new();

        [field: SerializeField]
        public Rigidbody Rigidbody { get; private set; }

        public float ActivatedTime { get; private set; }

        private void Awake()
        {
            itemHolder.OnItemModified += UpdateRenderer;
        }

        private void OnDestroy()
        {
            itemHolder.OnItemModified -= UpdateRenderer;
        }

        public void MovePosition(Vector3 direction)
        {
            Rigidbody.MovePosition(transform.position + direction);
        }

        public void Rotate(float angle)
        {
            _currentYRotation = Mathf.Repeat(_currentYRotation + angle, 360f);
            Rigidbody.MoveRotation(Quaternion.Euler(0, _currentYRotation, 0));
        }

        public void Init(ItemPacked item, Vector3 position, Vector3 pushForce)
        {
            ActivatedTime = Time.time;
            Rigidbody.position = position;
            Rigidbody.velocity = pushForce;
            itemHolder.SetItem(item);
        }

        public bool AddToIventory()
        {
            InventorySystem.Instance.AddItemToInventory(itemHolder);
            return itemHolder.IsEmpty();
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