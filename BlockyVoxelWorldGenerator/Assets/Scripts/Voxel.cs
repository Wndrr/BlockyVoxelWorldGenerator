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
    private readonly Chunk _parentChunk;
    private readonly WorldGeneratorSettings _settings;
    public GameObject gameObject;

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

    enum Cubeside
    {
        Down,
        Up,
        Left,
        Right,
        Front,
        Back
    }

    public Voxel(Vector3 localIdentifier, Vector3 chunkPosition, GameObject parent, Chunk parentChunk, WorldGeneratorSettings settings)
    {
        LocalIdentifier = localIdentifier;
        _parentChunk = parentChunk;
        _settings = settings;
        CreateGameObject(localIdentifier, chunkPosition, parent);
        var worldPosition = localIdentifier / _settings.blocksPerMeter + chunkPosition;
        var perlinNoise = Mathf.PerlinNoise(worldPosition.x * .02f + 10000, worldPosition.z * .02f + 10000); 
        var heightMap = Noise.Map(0, _settings.maxHeightInChunks * (float)settings.voxelsPerChunkSide / _settings.blocksPerMeter, 0, 1, perlinNoise);
        if (heightMap < worldPosition.y)
            isSolid = false;
        else
        {
            isSolid = true;
        }
    }

    private void CreateGameObject(Vector3 localIdentifier, Vector3 chunkPosition, GameObject parent)
    {
        gameObject = new GameObject
        {
            name = $"Voxel - {localIdentifier.ToString()}"
        };
        gameObject.transform.parent = parent.transform;
        gameObject.transform.position = (localIdentifier / _settings.blocksPerMeter) + chunkPosition;
        gameObject.transform.localScale = Vector3.one / _settings.blocksPerMeter;
    }

    public void CreateMesh()
    {
        if (!isSolid)
            return;

        foreach (var side in _sides)
        {
            if (!IsVoxelWithThisIdentifierSolid(LocalIdentifier + side.Value))
            {
                CreateFace(side.Key, side.Value);
            }
        }
    }

    int ConvertOffsetDimensionToTargetChunk(int i)
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
        Chunk chunk;
        if (IsNotInCurrentChunk(identifierOfVoxelToCheck))
        {
            var idOfTargetChunk = GetIdOfTargetChunk(identifierOfVoxelToCheck);

            try
            {
                chunk = WorldGenerator.Chunks[new Vector3Int((int) idOfTargetChunk.x, (int) idOfTargetChunk.y, (int) idOfTargetChunk.z)];
            }
            catch (Exception e)
            {
                return false;
            }
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

    private void CreateFace(Cubeside sideName, Vector3 sideDirection)
    {
        var mesh = new Mesh
        {
            name = "ScriptedMesh" + sideName
        };

        Vector3[] vertices;
        var normals = new[] {sideDirection, sideDirection, sideDirection, sideDirection};
        var triangles = new[] {3, 1, 0, 3, 2, 1};
        Vector2[] uvs = {Vector2.down, Vector2.left, Vector2.up, Vector2.right};
        // Bottom
        var p0 = new Vector3(-0.5f, -0.5f, 0.5f);
        var p1 = new Vector3(0.5f, -0.5f, 0.5f);
        var p2 = new Vector3(0.5f, -0.5f, -0.5f);
        var p3 = new Vector3(-0.5f, -0.5f, -0.5f);
        // Top
        var p4 = new Vector3(-0.5f, 0.5f, 0.5f);
        var p5 = new Vector3(0.5f, 0.5f, 0.5f);
        var p6 = new Vector3(0.5f, 0.5f, -0.5f);
        var p7 = new Vector3(-0.5f, 0.5f, -0.5f);

        switch (sideName)
        {
            case Cubeside.Down:
                vertices = new[] {p0, p1, p2, p3};
                break;
            case Cubeside.Up:
                vertices = new[] {p7, p6, p5, p4};
                break;
            case Cubeside.Left:
                vertices = new[] {p7, p4, p0, p3};
                break;
            case Cubeside.Right:
                vertices = new[] {p5, p6, p2, p1};
                break;
            case Cubeside.Front:
                vertices = new[] {p4, p5, p1, p0};
                break;
            case Cubeside.Back:
                vertices = new[] {p6, p7, p3, p2};
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sideName), sideName, null);
        }


        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        var quad = new GameObject("Quad");
        quad.transform.parent = gameObject.transform;
        quad.transform.position = LocalIdentifier;

        var meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}