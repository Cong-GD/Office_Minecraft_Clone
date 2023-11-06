
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Minecraft.ProceduralMeshGenerate
{
    public class ObjectMeshData
    {
        
        public readonly Mesh mesh;
        public readonly Material material;
        public readonly ItemRelaviteTransforms itemTransforms;

        public ObjectMeshData(Mesh mesh, Material material, ItemRelaviteTransforms itemTransforms)
        {
            this.mesh = mesh;
            this.material = material;
            this.itemTransforms = itemTransforms;
        }
        
    }

    public enum ItemTransformState
    {
        InRightHand,
        InLeftHand,
        FreeObject
    }

    [Serializable]
    public class ItemRelaviteTransforms
    {
        [Serializable]
        public class ItemTransform
        {
            public ItemTransformState state;
            public RelativeTransform transform;
        }

        [SerializeField]
        private RelativeTransform @default = new();

        [SerializeField]
        private List<ItemTransform> statedTransforms = new List<ItemTransform>();


        public RelativeTransform GetRelativeTransfrom(ItemTransformState state)
        {
           var itemTransform = statedTransforms.Find(t => t.state == state);
            return itemTransform == null ? @default : itemTransform.transform;
        }
    }

    [Serializable]
    public class RelativeTransform
    {
        [field: SerializeField]
        public Vector3 position { get; private set; }

        [field: SerializeField]
        public Vector3 rotation { get; private set; }

        [field: SerializeField]
        public Vector3 scale { get; private set; } = Vector3.one;

        public RelativeTransform()
        {
        }

        public RelativeTransform(Transform transform)
        {
            position = transform.localPosition;
            rotation = transform.localEulerAngles;
            scale = transform.localScale;
        }

        public RelativeTransform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void Apply(Transform transform)
        {
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }
    }
    
}
