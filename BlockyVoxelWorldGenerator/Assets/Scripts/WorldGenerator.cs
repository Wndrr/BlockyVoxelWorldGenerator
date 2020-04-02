using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Profiling;

public enum ChunkLoadState
{
    Done,
    AwaitDraw,
    Keep,
    Remove
}

[RequireComponent(typeof(WorldGeneratorSettings))]
public class WorldGenerator : MonoBehaviour
{
    public WorldGeneratorSettings settings;
    private Vector3 _position;
    public static List<ChunkData> ChunksData;
    public List<ChunkRenderer> ChunkRenderers;
    public GameObject mapGenerationCenter;

    private List<Vector3> _directions = new List<Vector3>()
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    private Vector3Int _identifierOfPreviousChunk;

    // Start is called before the first frame update
    void Start()
    {
        mapGenerationCenter.SetActive(false);
        Setup();
        // Generate initial chunk to display as fast as possible, other chunks will be generated later
        StartCoroutine(GenerateChunks(1));
        mapGenerationCenter.SetActive(true);
    }

    private Vector3Int GetCenterPointCurrentChunk()
    {
        return (mapGenerationCenter.transform.position / (settings.voxelsPerChunkSide * settings.blocksPerMeter)).ToVector3Int();
    }

    private void Setup()
    {
        var chunkCountHelper = new ChunkCountHelper();
        var chunkCount = chunkCountHelper.GetChunksCountForRadius(GetCenterPointCurrentChunk(), settings.generationRadiusInChunks);
        ChunksData = new List<ChunkData>(chunkCount);
        ChunkRenderers = new List<ChunkRenderer>(chunkCount);
        _identifierOfPreviousChunk = GetCenterPointCurrentChunk();
    }

    private IEnumerator GenerateChunks(int? overrideGenerationRadiusInChunks = null)
    {
        Profiler.BeginSample("MyPieceOfCode");
        Profiler.BeginSample(nameof(SetChunkKeepStates));
        var chunksToKeep = SetChunkKeepStates(overrideGenerationRadiusInChunks);
        Profiler.EndSample();
        
        Profiler.BeginSample(nameof(RemoveChunks));
        RemoveChunks();
        Profiler.EndSample();
        yield return null;
        
        Profiler.BeginSample(nameof(CreateNewChunksData));
        CreateNewChunksData(chunksToKeep);
        Profiler.EndSample();
        yield return null;
        
        Profiler.BeginSample(nameof(UpdateChunksData));
        UpdateChunksData();
        Profiler.EndSample();
        yield return null;

        Profiler.BeginSample(nameof(CreateChunkRenderers));
        CreateChunkRenderers();
        Profiler.EndSample();
        yield return null;
        
        Profiler.BeginSample(nameof(DrawChunks));
        DrawChunks();
        Profiler.EndSample();

        Profiler.EndSample();

    }

    private static void RemoveChunks()
    {
        ChunksData.RemoveAll(chunk => chunk.State == ChunkLoadState.Remove);
    }

    private void DrawChunks()
    {
        foreach (var chunkRenderer in ChunkRenderers.Where(renderer => renderer.State == ChunkRendererState.AwaitingDraw))
        {
            StartCoroutine(chunkRenderer.Draw());
        }
    }

    private void CreateChunkRenderers()
    {
        foreach (var chunkData in ChunksData.Where(c => c.State == ChunkLoadState.AwaitDraw))
        {
            ChunkRenderers.Add(new ChunkRenderer(chunkData, gameObject));
        }
    }

    private void UpdateChunksData()
    {
        var removedChunkIds = ChunksData.Where(chunk => chunk.State == ChunkLoadState.Remove).Select(s => s.Identifier).ToList();
        var chunkRederersToUpdate = ChunkRenderers.Where(chunk => removedChunkIds.Contains(chunk.Data.Identifier) || chunk.State == ChunkRendererState.Available).ToArray();
        var justGeneratedChunks = ChunksData.Where(chunk => chunk.State == ChunkLoadState.AwaitDraw).ToList();
        for (var i = 0; i < chunkRederersToUpdate.Count(); i++)
        {
            if (justGeneratedChunks.Count > i)
                chunkRederersToUpdate[i].UpdateData(justGeneratedChunks[i]);

            else
            {
                chunkRederersToUpdate[i].GameObject.SetActive(false);
                chunkRederersToUpdate[i].State = ChunkRendererState.Available;
            }
        }
    }

    private void CreateNewChunksData(List<Vector3Int> chunksToKeep)
    {
        var alreadyLoadedChunks = ChunksData.Select(s => s.Identifier);
        var chunksToLoad = chunksToKeep.Where(chunk => !alreadyLoadedChunks.Contains(chunk));
        foreach (var chunkToLoad in chunksToLoad)
        {
            ChunksData.Add(new ChunkData(chunkToLoad, settings));
        }
    }

    private List<Vector3Int> SetChunkKeepStates(int? overrideGenerationRadiusInChunks)
    {
        var centerOfGenerationChunkId = GetCenterPointCurrentChunk();
        var chunksToKeep = new ChunkKeepHelper().GetChunksToKeep(centerOfGenerationChunkId, overrideGenerationRadiusInChunks ?? settings.generationRadiusInChunks);
        foreach (var chunk in ChunksData)
        {
            if (chunksToKeep.Contains(chunk.Identifier))
                chunk.State = ChunkLoadState.Keep;
            else
                chunk.State = ChunkLoadState.Remove;
        }

        return chunksToKeep;
    }

    // private void OnDrawGizmos()
    // {
    //     foreach (var chunkData in ChunksData)
    //     {
    //         foreach (var voxel in chunkData.Voxels)
    //         {
    //             Gizmos.DrawSphere(voxel.LocalIdentifier + chunkData.Identifier.ToVector3(), .2f);
    //         }
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        var identifierOfCurrentChunk = (mapGenerationCenter.transform.position / (settings.voxelsPerChunkSide * settings.blocksPerMeter)).ToVector3Int();
        if (identifierOfCurrentChunk != _identifierOfPreviousChunk || !_hasFullGenBeenDone)
        {
            StartCoroutine(GenerateChunks());
            _identifierOfPreviousChunk = identifierOfCurrentChunk;
            _hasFullGenBeenDone = true;
        }
    }

    private bool _hasFullGenBeenDone = false;
}

public class ChunkCountHelper
{
    private readonly List<Vector3Int> _alreadyGeneratedChunks = new List<Vector3Int>();

    private readonly List<Vector3> _directions = new List<Vector3>()
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    public int GetChunksCountForRadius(Vector3Int currentPosition, int radius)
    {
        if (radius == 0)
            return 0;

        if (!_alreadyGeneratedChunks.Contains(currentPosition))
            _alreadyGeneratedChunks.Add(currentPosition);

        var directionsToCheck = _directions.Select(direction => (currentPosition.ToVector3() + direction).ToVector3Int());
        var chunksCountForRadius = directionsToCheck.Sum(positionToCheck => GetChunksCountForRadius(positionToCheck, radius - 1));
        return chunksCountForRadius;
    }
}

public class ChunkKeepHelper
{
    private readonly List<Vector3Int> _alreadyGeneratedChunks = new List<Vector3Int>();

    private readonly List<Vector3> _directions = new List<Vector3>()
    {
        Vector3.back,
        Vector3.forward,
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    public List<Vector3Int> GetChunksToKeep(Vector3Int currentPosition, int radius)
    {
        GenerateChunkList(currentPosition, radius);

        return _alreadyGeneratedChunks;
    }

    private void GenerateChunkList(Vector3Int currentPosition, int radius)
    {
        if (radius == 0)
            return;

        if (!_alreadyGeneratedChunks.Contains(currentPosition))
            _alreadyGeneratedChunks.Add(currentPosition);

        var directionsToCheck = _directions.Select(direction => (currentPosition.ToVector3() + direction).ToVector3Int()).ToList();
        directionsToCheck.ForEach(positionToCheck => GenerateChunkList(positionToCheck, radius - 1));
    }
}