using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(WorldGeneratorSettings))]
public class WorldGenerator : MonoBehaviour
{
    public WorldGeneratorSettings settings;
    private Vector3 _position;
    public static Dictionary<Vector3Int, Chunk> Chunks;

    private List<Vector3> _directions = new List<Vector3>()
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    // Start is called before the first frame update
    void Start()
    {
        Chunks = new Dictionary<Vector3Int, Chunk>(settings.generationRadiusInChunks * 2 * 3);
        StartCoroutine(nameof(GenerateChunks));
    }

    private void GenerateChunkAndAdjacentChunks(Vector3Int identifier, int remainingDistance)
    {
        if (!Chunks.ContainsKey(identifier))
            Chunks.Add(identifier, new Chunk(identifier, this.gameObject, settings));

        if (remainingDistance <= 0)
            return;

        foreach (var identifierOfAdjacentChunkToGenerate in _directions.Select(direction => identifier + direction))
        {
            GenerateChunkAndAdjacentChunks(identifierOfAdjacentChunkToGenerate.ToVector3Int(), remainingDistance - 1);
        }
    }

    private void GenerateChunks()
    {
        GenerateChunkAndAdjacentChunks(Vector3Int.zero, settings.generationRadiusInChunks - 1);

        foreach (var chunk in Chunks)
        {
            chunk.Value.CreateMesh();
        }
    }

    private void OnDrawGizmos()
    {
        if (!settings.IsDebug)
            return;

        for (var chunkX = 0; chunkX < Chunks.Count; chunkX++)
        for (var chunkY = 0; chunkY < Chunks.Count; chunkY++)
        for (var chunkZ = 0; chunkZ < Chunks.Count; chunkZ++)
        {
            try
            {
                var chunk = Chunks[new Vector3Int(chunkX, chunkY, chunkZ)];
                if (chunk != null)
                {
                    for (var voxelX = 0; voxelX < chunk.Voxels.GetLength(0); voxelX++)
                    for (var voxelY = 0; voxelY < chunk.Voxels.GetLength(1); voxelY++)
                    for (var voxelZ = 0; voxelZ < chunk.Voxels.GetLength(2); voxelZ++)
                    {
                        var position = VoxelConverter.GetWorldPosition(chunk.Identifier, chunk.Voxels[voxelX, voxelY, voxelZ].LocalIdentifier, settings);
                        Gizmos.DrawSphere(position, .1f / settings.blocksPerMeter);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var chunk in Chunks)
            {
                Destroy(chunk.Value.gameObject);
            }

            Chunks.Clear();

            Chunks = new Dictionary<Vector3Int, Chunk>(settings.generationRadiusInChunks * 2 * 3);
            StartCoroutine(nameof(GenerateChunks));
        }
    }
}

public static class Vector3Utils
{
    public static Vector3Int ToVector3Int(this Vector3 input)
    {
        return new Vector3Int()
        {
            x = (int) input.x,
            y = (int) input.y,
            z = (int) input.z,
        };
    }
}