using NaughtyAttributes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{

    public abstract class BlockMeshDataGenerator_SO : ScriptableObject
    {
        [SerializeField]
        private Material itemMaterialPrefab;

        public abstract void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z);

        public Material CreateMaterial()
        {
            var material = new Material(itemMaterialPrefab);
            ObjectManager.AddToManagingList(material);
            return material;
        }

        public abstract Mesh CreateMesh();

        public Mesh CreateMeshWithoutUvAtlas()
        {
            var mesh = CreateMesh();
            if(mesh == null)
            {
                return null;
            }

            Vector2[] uvs = mesh.uv;
            Span<Vector2> span = uvs;
            foreach (ref var uv in span)
            {
                uv *= MeshDrawerHelper.ATLAS_SIZE;
            }
            mesh.uv = uvs;
            return mesh;
        }
    }
}
