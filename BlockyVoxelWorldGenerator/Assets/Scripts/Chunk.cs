using UnityEngine;

public class Chunk
{
    public readonly Vector3 Identifier;
    private readonly WorldGeneratorSettings _settings;
    public Voxel[,,] Voxels;
    public GameObject gameObject;

    public Chunk(Vector3 identifier, GameObject parent, WorldGeneratorSettings settings)
    {
        Identifier = identifier;
        _settings = settings;
        gameObject = new GameObject
        {
            name = $"Chunk - {identifier.ToString()}"
        };
        gameObject.transform.parent = parent.transform;
        CreateVoxels(settings);
        MergeVoxelMeshes();
        gameObject.transform.position = identifier * settings.voxelsPerChunkSide / settings.blocksPerMeter;
    }

    private void CreateVoxels(WorldGeneratorSettings settings)
    {
        Voxels = new Voxel[settings.voxelsPerChunkSide, settings.voxelsPerChunkSide, settings.voxelsPerChunkSide];
        for (var x = 0; x < settings.voxelsPerChunkSide; x++)
        for (var y = 0; y < settings.voxelsPerChunkSide; y++)
        for (var z = 0; z < settings.voxelsPerChunkSide; z++)
        {
            var localIdentifier = new Vector3(x, y, z);
            Voxels[x, y, z] = new Voxel(localIdentifier, gameObject.transform.position, gameObject, settings);
        }
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
        var voxelMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        var meshCombiner = new CombineInstance[voxelMeshFilters.Length];
        var i = 0;
        while (i < voxelMeshFilters.Length)
        {
            meshCombiner[i].mesh = voxelMeshFilters[i].sharedMesh;
            meshCombiner[i].transform = voxelMeshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        return meshCombiner;
    }
}