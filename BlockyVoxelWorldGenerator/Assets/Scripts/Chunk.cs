using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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
        _meshFilter.transform.position = Vector3.zero;
        _meshRenderer = GameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = WorldGeneratorSettings.DefaultMaterial;
        State = ChunkRendererState.AwaitingDraw;
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
    
    public void Draw()
    {
        UpdateMesh();
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

    private void UpdateMesh()
    {
        CreateVoxelsMeshData();
        var meshCombiner = new CombineInstance[Data.Voxels.Length * 6];
        var i = 0;
        var chunkTransform = GameObject.transform;
        foreach (var voxel in Data.Voxels)
        {
            for (var j = 0; j < voxel.Vertices.Length; j++)
            {
                AddMeshToCombiner(i, j, voxel, meshCombiner, chunkTransform);
            }

            i += voxel.Vertices.Length;
        }

        _meshFilter.mesh.Clear();
        _meshFilter.mesh.CombineMeshes(meshCombiner);
    }

    private static void AddMeshToCombiner(int i, int j, Voxel voxel, CombineInstance[] meshCombiner1, Transform chunkTransform)
    {
        var current = i + j;
        var mesh = new Mesh()
        {
            vertices = voxel.Vertices[j],
            triangles = voxel.Triangles[j],
            uv = voxel.Uv[j],
            normals = voxel.Normals[j],
        };
        meshCombiner1[current].mesh = mesh;
        meshCombiner1[current].transform = chunkTransform.localToWorldMatrix;
    }
}