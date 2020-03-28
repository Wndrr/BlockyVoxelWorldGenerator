using UnityEngine;

public class Chunk
{
    public readonly Vector3 Identifier;
    public Voxel[,,] Voxels;

    public Chunk(WorldGeneratorSettings worldGeneratorSettings, Vector3 identifier)
    {
        Identifier = identifier;
        Voxels = new Voxel[worldGeneratorSettings.voxelsPerChunkSide, worldGeneratorSettings.voxelsPerChunkSide, worldGeneratorSettings.voxelsPerChunkSide];
        for (var x = 0; x < worldGeneratorSettings.voxelsPerChunkSide; x++)
        for (var y = 0; y < worldGeneratorSettings.voxelsPerChunkSide; y++)
        for (var z = 0; z < worldGeneratorSettings.voxelsPerChunkSide; z++)
        {
            Voxels[x, y, z] = new Voxel(new Vector3(x, y, z));
        }
    }
}