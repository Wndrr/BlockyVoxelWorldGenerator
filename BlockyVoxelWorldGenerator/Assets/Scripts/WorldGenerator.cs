using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldGeneratorSettings))]
public class WorldGenerator : MonoBehaviour
{
    public WorldGeneratorSettings settings;
    private Vector3 _position;
    private Chunk[,,] chunks;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Chunk[settings.generationRadiusInChunks, settings.generationRadiusInChunks, settings.generationRadiusInChunks];
        StartCoroutine(nameof(GenerateChunks));
    }

    private IEnumerator GenerateChunks()
    {
        for (var x = 0; x < settings.generationRadiusInChunks; x++)
        for (var y = 0; y < settings.generationRadiusInChunks; y++)
        for (var z = 0; z < settings.generationRadiusInChunks; z++)
        {
            chunks[x, y, z] = new Chunk(new Vector3(x, y, z), this.gameObject, settings);
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!settings.IsDebug)
            return;

        for (var chunkX = 0; chunkX < chunks.GetLength(0); chunkX++)
        for (var chunkY = 0; chunkY < chunks.GetLength(1); chunkY++)
        for (var chunkZ = 0; chunkZ < chunks.GetLength(2); chunkZ++)
        {
            try
            {
                var chunk = chunks[chunkX, chunkY, chunkZ];
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
            for (var x = 0; x < chunks.GetLength(0); x++)
            for (var y = 0; y < chunks.GetLength(1); y++)
            for (var z = 0; z < chunks.GetLength(2); z++)
            {
             Destroy(chunks[x, y, z].gameObject);   
            }
            chunks = new Chunk[settings.generationRadiusInChunks, settings.generationRadiusInChunks, settings.generationRadiusInChunks];
            StartCoroutine(nameof(GenerateChunks));
        }
    }
}