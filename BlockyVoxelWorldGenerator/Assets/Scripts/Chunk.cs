using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


    public void CreateVoxelsMeshData()
    {
        var tasks = new List<Task>(Data.Voxels.Length);
        tasks.AddRange(Data.Voxels.Cast<Voxel>().Select(voxel => Task.Run(voxel.CreateMesh)));

        Task.WaitAll(tasks.ToArray());
    }

    public IEnumerator Draw()
    {
        yield return UpdateMesh();
        UpdateTransform();
        State = ChunkRendererState.Drawn;
    }

    private void UpdateTransform()
    {
        //A local reference is faster than accessing the built-in prop
        var transformProp = GameObject.transform;
        transformProp.position = Data.Identifier.ToVector3() * (Data._settings.voxelsPerChunkSide / (float) Data._settings.blocksPerMeter);
        transformProp.localScale /= Data._settings.blocksPerMeter;
    }

    private CombineInstance[] _meshCombiner;

    private IEnumerator UpdateMesh()
    {
        CreateVoxelsMeshData();
        yield return null;
        var numberOfMeshesToCombine = Data.Voxels.Length * 6;
        if (_meshCombiner == null || _meshCombiner.Length < numberOfMeshesToCombine)
        {
            _meshCombiner = new CombineInstance[numberOfMeshesToCombine];
            for (var index = 0; index < _meshCombiner.Length; index++)
            {
                _meshCombiner[index].mesh = new Mesh();
            }
        }


        var tasks = new List<Task>(Data.Voxels.Length);
        var i = 0;
        foreach (var voxel in Data.Voxels)
        {
            if (voxel.HasAtLeasOneNonSolidNeighbourgh)
            {
                var i1 = i;
                tasks.Add(Task.Run(() => { CombienVoxelMeshes(voxel, i1); }));
            }

            i += voxel.Vertices.Length;
            if (i % (Data.Voxels.Length / 10) == 0)
                yield return null;
        }

        var allTasks = Task.WhenAll(tasks);
        while (!allTasks.IsCompleted)
        {
            yield return null;
        }
        
        _meshFilter.mesh.Clear();
        _meshFilter.mesh.CombineMeshes(_meshCombiner);
    }

    private void CombienVoxelMeshes(Voxel voxel, int i1)
    {
        for (var j = 0; j < voxel.Vertices.Length; j++)
        {
            var mesh = _meshCombiner[i1 + j].mesh;
            mesh.Clear();
            MapIntoMesh(mesh, voxel, j);
            _meshCombiner[i1 + j].transform = Matrix4x4.identity;
        }
    }

    private static void MapIntoMesh(Mesh mesh, Voxel voxel, int j)
    {
        mesh.vertices = voxel.Vertices[j];
        mesh.triangles = voxel.Triangles[j];
        mesh.uv = voxel.Uv[j];
        mesh.normals = voxel.Normals[j];
    }
}