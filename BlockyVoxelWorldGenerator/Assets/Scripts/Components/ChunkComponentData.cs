using BlitableRedefinitions;
using Unity.Entities;

namespace Components
{
    public struct ChunkComponentData : IComponentData
    {
        //public VoxelData[] Voxels { get; set; }
        public BlitableBool NeedsRender { get; set; }
    }
    
    public enum VoxelType
    {
        Air,
        Stone
    }

    public struct VoxelData
    {
        public int Type { get; set; }
    }
}