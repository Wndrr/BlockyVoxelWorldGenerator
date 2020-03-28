using UnityEngine;

public class Voxel
{
    public readonly Vector3 LocalIdentifier;
    public GameObject gameObject;

    public Voxel(Vector3 localIdentifier, Vector3 chunkPosition, GameObject parent, WorldGeneratorSettings settings)
    {
        gameObject = new GameObject();
        gameObject.name = $"Voxel - {localIdentifier.ToString()}";
        gameObject.transform.parent = parent.transform;
        gameObject.transform.position =  (localIdentifier / settings.blocksPerMeter) + chunkPosition ;
        gameObject.transform.localScale = Vector3.one / settings.blocksPerMeter;
        LocalIdentifier = localIdentifier;
        var primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.AddComponent<MeshFilter>().mesh = primitive.GetComponent<MeshFilter>().mesh;
        gameObject.AddComponent<MeshRenderer>().material = primitive.GetComponent<MeshRenderer>().material;
        GameObject.Destroy(primitive);
    }
}