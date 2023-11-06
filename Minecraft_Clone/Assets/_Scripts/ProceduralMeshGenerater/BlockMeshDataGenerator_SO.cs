using NaughtyAttributes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public abstract class BlockMeshDataGenerator_SO : ScriptableObject
    {
        [SerializeField]
        private Material materialPrefab;

        [NonSerialized]
        private Material _material;

        [NonSerialized]
        private Mesh _mesh;

        [SerializeField]
        protected ItemRelaviteTransforms statedTransforms;

        public abstract void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z);

        public ObjectMeshData GetObjectMeshData()
        {
            if(_mesh == null)
            {
                _mesh = GenerateObjectMesh();
                ObjectManager.AddToManagingList(_mesh);
            }
            if(_material == null)
            {
                _material = new Material(materialPrefab);
                ObjectManager.AddToManagingList(_material);
            }
            return new ObjectMeshData(_mesh, _material, statedTransforms);
        }

        protected abstract Mesh GenerateObjectMesh();
        
    }
}
