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

        public abstract void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z);

        public Material CreateMaterial()
        {
            var material = new Material(materialPrefab);
            ObjectManager.AddToManagingList(material);
            return material;
        }

        public abstract Mesh CreateMesh();

    }
}
