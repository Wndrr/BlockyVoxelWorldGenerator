using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Voxel
{
    public readonly Vector3 LocalIdentifier;
    private readonly ChunkData _parentChunk;
    private readonly WorldGeneratorSettings _settings;

    public Vector3[][] Vertices { get; set; }
    public Vector3[][] Normals { get; set; }
    public Vector2[][] Uv { get; set; }
    public int[][] Triangles { get; set; }
    public bool HasAtLeasOneNonSolidNeighbourgh { get; set; }

    private readonly Dictionary<Cubeside, Vector3> _sides = new Dictionary<Cubeside, Vector3>
    {
        {Cubeside.Back, Vector3.back},
        {Cubeside.Front, Vector3.forward},
        {Cubeside.Right, Vector3.right},
        {Cubeside.Left, Vector3.left},
        {Cubeside.Up, Vector3.up},
        {Cubeside.Down, Vector3.down}
    };

    public bool isSolid = true;

    public enum Cubeside
    {
        Down,
        Up,
        Left,
        Right,
        Front,
        Back
    }

    public Voxel(Vector3 localIdentifier, Vector3 chunkPosition, ChunkData parentChunk, WorldGeneratorSettings settings)
    {
        LocalIdentifier = localIdentifier;
        _parentChunk = parentChunk;
        _settings = settings;
        Vertices = new Vector3[6][];
        Normals = new Vector3[6][];
        Uv = new Vector2[6][];
        Triangles = new int[6][];
        var worldPosition = localIdentifier / _settings.blocksPerMeter + chunkPosition;

        var perlinNoise = Mathf.PerlinNoise(worldPosition.x * .02f + 10000, worldPosition.z * .02f + 10000);

        var heightMap = Noise.Map(0, _settings.maxHeightInChunks * (float) settings.voxelsPerChunkSide / _settings.blocksPerMeter, 0, 1, perlinNoise);
        if (heightMap < worldPosition.y)
            isSolid = false;
        else
        {
            isSolid = true;
        }
    }

    public IEnumerable<KeyValuePair<Cubeside, Vector3>> GetListOfFacesToDraw()
    {
        return _sides.Where(side => !IsVoxelWithThisIdentifierSolid(LocalIdentifier + side.Value));
        // return _sides;
    }

    private int ConvertOffsetDimensionToTargetChunk(int i)
    {
        if (i == -1)
            i = _settings.voxelsPerChunkSide - 1;
        else if (i == _settings.voxelsPerChunkSide)
            i = 0;
        return i;
    }

    private Vector3 ConvertOffsetPositionToTargetChunk(Vector3 inputPosition)
    {
        return new Vector3(
            ConvertOffsetDimensionToTargetChunk((int) inputPosition.x),
            ConvertOffsetDimensionToTargetChunk((int) inputPosition.y),
            ConvertOffsetDimensionToTargetChunk((int) inputPosition.z)
        );
    }

    private bool IsVoxelWithThisIdentifierSolid(Vector3 identifierOfVoxelToCheck)
    {
        var targetIdentifierToCheck = identifierOfVoxelToCheck;
        ChunkData chunk;
        if (IsNotInCurrentChunk(identifierOfVoxelToCheck))
        {
            var idOfTargetChunk = GetIdOfTargetChunk(identifierOfVoxelToCheck);

            chunk = WorldGenerator.ChunksData.SingleOrDefault(c => c.Identifier == idOfTargetChunk.ToVector3Int());
            if (chunk == null)
                return false;
        }
        else
        {
            chunk = _parentChunk;
        }

        try
        {
            return chunk.Voxels[(int) targetIdentifierToCheck.x, (int) targetIdentifierToCheck.y, (int) targetIdentifierToCheck.z].isSolid;
        }
        catch (Exception)
        {
            /* ignored*/
        }

        return false;
    }

    private Vector3 GetIdOfTargetChunk(Vector3 identifierOfVoxelToCheck)
    {
        var targetId = identifierOfVoxelToCheck;
        if (targetId.x < 0)
            targetId.x = _settings.voxelsPerChunkSide - 1;
        if (targetId.y < 0)
            targetId.y = _settings.voxelsPerChunkSide - 1;
        if (targetId.z < 0)
            targetId.z = _settings.voxelsPerChunkSide - 1;

        if (targetId.x > _settings.voxelsPerChunkSide)
            targetId.x = 0;
        if (targetId.y > _settings.voxelsPerChunkSide)
            targetId.y = 0;
        if (targetId.z > _settings.voxelsPerChunkSide)
            targetId.z = 0;

        return targetId;
    }

    private bool IsNotInCurrentChunk(Vector3 identifierToCheck)
    {
        return identifierToCheck.x < 0 || identifierToCheck.x >= _settings.voxelsPerChunkSide ||
               identifierToCheck.y < 0 || identifierToCheck.y >= _settings.voxelsPerChunkSide ||
               identifierToCheck.z < 0 || identifierToCheck.z >= _settings.voxelsPerChunkSide;
    }
}

public struct VoxelMeshData
{
    public Vector3[] Vertices { get; set; }
    public Vector3[] Normals { get; set; }
    public Vector2[] Uv { get; set; }
    public int[] Triangles { get; set; }
}