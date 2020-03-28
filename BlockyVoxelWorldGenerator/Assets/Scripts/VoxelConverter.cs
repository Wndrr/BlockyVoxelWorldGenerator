using UnityEngine;

public static class VoxelConverter
{
    public static Vector3 GetWorldPosition(Vector3 chunkIdentifier, Vector3 voxelPosition, WorldGeneratorSettings settings)
    {
        var chunkWorldPosition = chunkIdentifier * settings.voxelsPerChunkSide / settings.blocksPerMeter;
        var voxelLocalPosition = voxelPosition / settings.blocksPerMeter;

        return chunkWorldPosition + voxelLocalPosition;
    }
}