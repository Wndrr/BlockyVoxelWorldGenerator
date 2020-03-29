using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk
{
    public readonly Vector3 Identifier;
    private readonly WorldGeneratorSettings _settings;
    public Voxel[,,] Voxels;
    public GameObject gameObject;
    private Vector3 _chunkPosition;

    public Chunk(Vector3Int identifier, GameObject parent, WorldGeneratorSettings settings)
    {
        Identifier = identifier;
        _settings = settings;
        _chunkPosition = Identifier * _settings.voxelsPerChunkSide / _settings.blocksPerMeter;
        gameObject = new GameObject
        {
            name = $"Chunk - {identifier.ToString()}"
        };
        gameObject.transform.parent = parent.transform;
        CreateVoxels(settings);
    }

    private void CreateVoxels(WorldGeneratorSettings settings)
    {
        Voxels = new Voxel[settings.voxelsPerChunkSide, settings.voxelsPerChunkSide, settings.voxelsPerChunkSide];
        for (var x = 0; x < settings.voxelsPerChunkSide; x++)
        for (var y = 0; y < settings.voxelsPerChunkSide; y++)
        for (var z = 0; z < settings.voxelsPerChunkSide; z++)
        {
            var localIdentifier = new Vector3(x, y, z);
            Voxels[x, y, z] = new Voxel(localIdentifier, _chunkPosition, gameObject, this, settings);
        }

    }

    public void CreateMesh()
    {
        var tasks = new List<Task>(Voxels.Length);
        tasks.AddRange(Voxels.Cast<Voxel>().Select(voxel => Task.Run(voxel.CreateMesh)));

        Task.WaitAll(tasks.ToArray());
        MergeVoxelMeshes();
        gameObject.transform.position = _chunkPosition;
        gameObject.transform.localScale /= _settings.blocksPerMeter;
    }

    private void MergeVoxelMeshes()
    {
        var meshCombiner = GetMeshCombinerFromChildren();
        AddCombinedMeshesToChunk(meshCombiner);
        AddMeshRenderer();
        RemoveChildren();
    }

    private void RemoveChildren()
    {
        foreach (Transform quad in gameObject.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }
    }

    private void AddMeshRenderer()
    {
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Diffuse"));
    }

    private void AddCombinedMeshesToChunk(CombineInstance[] meshCombiner)
    {
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.transform.position = Vector3.zero;

        meshFilter.mesh.CombineMeshes(meshCombiner);
    }

    private CombineInstance[] GetMeshCombinerFromChildren()
    {
        var voxelMeshFilters = Voxels;
        var meshCombiner = new CombineInstance[voxelMeshFilters.Length * 6];
        var i = 0;
        var chunkTransform = gameObject.transform;
        foreach (var voxel in Voxels)
        {
            for (var j = 0; j < voxel.Vertices.Length; j++)
            {
                var current = i + j;
                var mesh = new Mesh()
                {
                    vertices = voxel.Vertices[j],
                    triangles = voxel.Triangles[j],
                    uv = voxel.Uv[j],
                    normals = voxel.Normals[j],
                };
                meshCombiner[current].mesh = mesh;
                meshCombiner[current].transform = chunkTransform.localToWorldMatrix;
            }
            i += voxel.Vertices.Length;
        }

        return meshCombiner;
    }
}