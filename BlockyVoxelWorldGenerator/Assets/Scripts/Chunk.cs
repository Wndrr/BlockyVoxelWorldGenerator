using UnityEngine;

public class Chunk
{
    public readonly Vector3 Identifier;
    public Voxel[,,] Voxels;
    public GameObject gameObject;

    public Chunk(Vector3 identifier, GameObject parent, WorldGeneratorSettings settings)
    {
        gameObject = new GameObject();
        gameObject.name = $"Chunk - {identifier.ToString()}";
        gameObject.transform.parent = parent.transform;
        gameObject.transform.position = identifier * settings.voxelsPerChunkSide / settings.blocksPerMeter;
        Identifier = identifier;
        Voxels = new Voxel[settings.voxelsPerChunkSide, settings.voxelsPerChunkSide, settings.voxelsPerChunkSide];
        for (var x = 0; x < settings.voxelsPerChunkSide; x++)
        for (var y = 0; y < settings.voxelsPerChunkSide; y++)
        for (var z = 0; z < settings.voxelsPerChunkSide; z++)
        {
            var localIdentifier = new Vector3(x, y, z);
            Voxels[x, y, z] = new Voxel(localIdentifier, gameObject.transform.position, gameObject, settings);
        }
    }
}