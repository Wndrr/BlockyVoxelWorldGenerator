using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public struct ChunkComponentData : IComponentData
    {
        public VoxelData[] Voxels { get; set; }
        public bool NeedsRender { get; set; }
    }
    
    public enum VoxelType
    {
        Air,
        Stone
    }

    public struct VoxelData
    {
        public VoxelType Type { get; set; }
    }
}