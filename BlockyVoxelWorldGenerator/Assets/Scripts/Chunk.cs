using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class ChunkData
{
    public readonly WorldGeneratorSettings _settings;
    public Voxel[,,] Voxels;
    public Vector3Int Identifier;
    private Vector3 _chunkPosition;
    public ChunkLoadState State { get; set; }

    public ChunkData(Vector3Int identifier, WorldGeneratorSettings settings)
    {
        State = ChunkLoadState.AwaitDraw;
        _settings = settings;
        Update(identifier);
        CreateVoxels(settings);
    }

    public void Update(Vector3Int identifier)
    {
        Identifier = identifier;
        _chunkPosition = Identifier.ToVector3() * _settings.voxelsPerChunkSide / _settings.blocksPerMeter;
        Identifier = identifier;
    }

    private void CreateVoxels(WorldGeneratorSettings settings)
    {
        Voxels = new Voxel[settings.voxelsPerChunkSide, settings.voxelsPerChunkSide, settings.voxelsPerChunkSide];
        for (var x = 0; x < settings.voxelsPerChunkSide; x++)
        for (var y = 0; y < settings.voxelsPerChunkSide; y++)
        for (var z = 0; z < settings.voxelsPerChunkSide; z++)
        {
            var localIdentifier = new Vector3(x, y, z);
            Voxels[x, y, z] = new Voxel(localIdentifier, _chunkPosition, this, settings);
        }
    }
}

public enum ChunkRendererState
{
    Available,
    AwaitingDraw,
    Drawn
}

public class ChunkRenderer
{
    public ChunkData Data { get; set; }

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    public ChunkRendererState State { get; set; }
    public GameObject GameObject;

    public ChunkRenderer(ChunkData data, GameObject parent)
    {
        GameObject = new GameObject();
        Data = data;
        GameObject.transform.parent = parent.transform;
        UpdateName();
        _meshFilter = GameObject.AddComponent<MeshFilter>();
        _meshRenderer = GameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = WorldGeneratorSettings.DefaultMaterial;
        State = ChunkRendererState.AwaitingDraw;
        UpdateTransform();
    }

    public void UpdateData(ChunkData data)
    {
        State = ChunkRendererState.AwaitingDraw;
        Data = data;
        Data.State = ChunkLoadState.Done;
        UpdateName();
    }

    private void UpdateName()
    {
        GameObject.name = $"Chunk - {Data.Identifier.ToString()}";
    }


    public IEnumerator Draw()
    {
        UpdateTransform();
        State = ChunkRendererState.Drawn;
        yield return UpdateMesh();
    }

    private void UpdateTransform()
    {
        //A local reference is faster than accessing the built-in prop
        var transformProp = GameObject.transform;
        transformProp.position = Data.Identifier.ToVector3() * (Data._settings.voxelsPerChunkSide / (float) Data._settings.blocksPerMeter);
        transformProp.localScale /= Data._settings.blocksPerMeter;
    }

    private List<VoxelMeshData> _voxelMeshDataPool;
    
    private IEnumerator UpdateMesh()
    {
        var maximumNumbersOfFaces = Data.Voxels.Length * 6;
        if (maximumNumbersOfFaces > 65000)
            throw new NotImplementedException("Mesh with more than 65 000 vertices are not supported");

        if (_voxelMeshDataPool == null || _voxelMeshDataPool.Count != maximumNumbersOfFaces)
            _voxelMeshDataPool = new List<VoxelMeshData>(maximumNumbersOfFaces);

        var tasks = new List<Task>();
        
        foreach (var voxel in Data.Voxels)
        {
            tasks.Add(Task.Run(() => GenerateVoxelFaces(voxel)));
        }

        var tasksTask = Task.WhenAll(tasks);
        while (!tasksTask.IsCompleted)
            yield return null;

        DrawMesh();
    }

    private void GenerateVoxelFaces(Voxel voxel)
    {
        var listOfFacesToDraw = voxel.GetListOfFacesToDraw().ToList();
        foreach (var side in listOfFacesToDraw)
        {
            Vector3[] vertices;
            int[] triangles;
            
            var leftDownBack = (Vector3.left + Vector3.down + Vector3.back) / 2;
            var rightDownBack = (Vector3.right + Vector3.down + Vector3.back) / 2;
            var rightUpBack = (Vector3.right + Vector3.up + Vector3.back) / 2;
            var leftUpBack = (Vector3.left + Vector3.up + Vector3.back) / 2;
            var leftUpForward = (Vector3.left + Vector3.up + Vector3.forward) / 2;
            var rightUpForward = (Vector3.right + Vector3.up + Vector3.forward) / 2;
            var rightDownForward = (Vector3.right + Vector3.down + Vector3.forward) / 2;
            var leftDownForward = (Vector3.left + Vector3.down + Vector3.forward) / 2;

            switch (side.Key)
            {
                case Voxel.Cubeside.Front:
                    vertices = new[] {leftUpForward, rightUpForward, rightDownForward, leftDownForward};
                    triangles = new[] {1, 0, 3, 1, 3, 2,};
                    break;
                case Voxel.Cubeside.Back:
                    vertices = new[] {leftDownBack, rightDownBack, rightUpBack, leftUpBack,};
                    triangles = new[] {0, 2, 1, 0, 3, 2,};
                    break;
                case Voxel.Cubeside.Up:
                    vertices = new[] {rightUpBack, leftUpBack, leftUpForward, rightUpForward};
                    triangles = new[] {0, 1, 2, 0, 2, 3,};
                    break;
                case Voxel.Cubeside.Down:
                    vertices = new[] {leftDownBack, rightDownBack, rightDownForward, leftDownForward};
                    triangles = new[] {0, 2, 3, 0, 1, 2};
                    break;
                case Voxel.Cubeside.Right:
                    vertices = new[] {rightDownBack, rightUpBack, rightUpForward, rightDownForward};
                    triangles = new[] {0, 1, 2, 0, 2, 3,};
                    break;
                case Voxel.Cubeside.Left:
                    vertices = new[] {leftDownBack, leftUpBack, leftUpForward, leftDownForward};
                    triangles = new[] {0, 3, 2, 0, 2, 1,};
                    break;

                default:
                    continue;
                // throw new ArgumentOutOfRangeException(nameof(side.Key), side.Key, null);
            }

            if (vertices.Length == 0)
                continue;
            var normales = new[] {side.Value, side.Value, side.Value, side.Value,};
            var localIdentifier = voxel.LocalIdentifier.ToVector3Int();
            vertices = vertices.Select(v => v + localIdentifier).ToArray();
            lock (_voxelMeshDataPool)
            {
                var data = new VoxelMeshData
                {
                    Vertices = vertices,
                    Triangles = triangles,
                    Normals = normales,
                    Side = side.Key
                };
                _voxelMeshDataPool.Add(data);
            }
        }
    }


    private void DrawMesh()
    {
        var voxelMeshDataPool = _voxelMeshDataPool.Where(m => m.Normals != null & m.Triangles != null && m.Triangles != null).ToArray();
        var vertices = voxelMeshDataPool.SelectMany(v => v.Vertices).ToArray();
        var vector3s = voxelMeshDataPool.SelectMany(v => v.Normals).ToArray();
        var triangles = new List<int>();
        for (var i = 0; i < voxelMeshDataPool.Length; i++)
        {
            triangles.AddRange(voxelMeshDataPool[i].Triangles.Select(triangle => triangle + (4 * i)));
        }

        var meshFilterMesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles.ToArray(),
            normals = vector3s,
            uv = new Vector2[0]
        };
        // MeshUtility.Optimize(meshFilterMesh);
        meshFilterMesh.RecalculateBounds();
        meshFilterMesh.RecalculateNormals();
        meshFilterMesh.RecalculateTangents();
        _meshFilter.mesh = meshFilterMesh;
    }
}