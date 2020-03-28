using System;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    public readonly Vector3 LocalIdentifier;
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
    
    enum Cubeside {Down, Up, Left, Right, Front, Back}

    public Voxel(Vector3 localIdentifier, Vector3 chunkPosition, GameObject parent, WorldGeneratorSettings settings)
    {
        LocalIdentifier = localIdentifier;
        _settings = settings;
        CreateGameObject(localIdentifier, chunkPosition, parent);
        CreateMesh();
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

    private void CreateMesh()
    {
        foreach (var side in _sides)
        {
            CreateFace(side.Key, side.Value);
        }
    }

    private void CreateFace(Cubeside sideName, Vector3 sideDirection)
    {
        var mesh = new Mesh
        {
            name = "ScriptedMesh" + sideName
        };

        Vector3[] vertices;
        var normals = new[] {sideDirection, sideDirection, sideDirection, sideDirection};
        var triangles = new[] { 3, 1, 0, 3, 2, 1};
        Vector2[] uvs = {Vector2.down, Vector2.left, Vector2.up, Vector2.right};
        // Bottom
        var p0 = new Vector3( -0.5f,  -0.5f,  0.5f );
        var p1 = new Vector3(  0.5f,  -0.5f,  0.5f );
        var p2 = new Vector3(  0.5f,  -0.5f, -0.5f );
        var p3 = new Vector3( -0.5f,  -0.5f, -0.5f );	
        // Top
        var p4 = new Vector3( -0.5f,   0.5f,  0.5f );
        var p5 = new Vector3(  0.5f,   0.5f,  0.5f );
        var p6 = new Vector3(  0.5f,   0.5f, -0.5f );
        var p7 = new Vector3( -0.5f,   0.5f, -0.5f );

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