using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public class CreateChunkMesh : ComponentSystem 
    {
        
        protected override void OnUpdate()
        {
            var material = new Material(Shader.Find("Diffuse"));
            material.color = Color.magenta;
            var entities = World.Active.GetOrCreateManager<EntityManager>().GetAllEntities();
            for (int i = 0; i < entities.Length; i++)
            {
                var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entities[i]);
                var positions = EntityManager.GetComponentData<Position>(entities[i]);
                renderMesh.material = material;
                var mesh = UpdateMesh(positions);
                renderMesh.mesh = mesh;
                EntityManager.SetSharedComponentData(entities[i], renderMesh);
            }
        }
        
        private VoxelMeshData[] _voxelMeshDataPool;
        
        public struct VoxelMeshData
        {
            public Vector3[] Vertices { get; set; }
            public Vector3[] Normals { get; set; }
            public Vector2[] Uv { get; set; }
            public int[] Triangles { get; set; }
        }
        public IEnumerable<KeyValuePair<Cubeside, Vector3>> GetListOfFacesToDraw()
        {
            // return _sides.Where(side => !IsVoxelWithThisIdentifierSolid(LocalIdentifier + side.Value));
            return _sides;
        }
        private Mesh UpdateMesh(Position positions)
    {
        _voxelMeshDataPool = new VoxelMeshData[6];
        var i = 0;
            foreach (var side in GetListOfFacesToDraw())
            {
                Vector3[] vertices1;
                int[] triangles;
                var leftDownBack = Vector3.left + Vector3.down + Vector3.back;
                var rightDownBack = Vector3.right + Vector3.down + Vector3.back;
                var rightUpBack = Vector3.right + Vector3.up + Vector3.back;
                var leftUpBack = Vector3.left + Vector3.up + Vector3.back;
                var leftUpForward = Vector3.left + Vector3.up + Vector3.forward;
                var rightUpForward = Vector3.right + Vector3.up + Vector3.forward;
                var rightDownForward = Vector3.right + Vector3.down + Vector3.forward;
                var leftDownForward = Vector3.left + Vector3.down + Vector3.forward;

                switch (side.Key)
                {
                    case Cubeside.Front:
                        vertices1 = new[] {leftDownBack, rightDownBack, rightUpBack, leftUpBack,};
                        triangles = new[] {0, 2, 1, 0, 3, 2,};
                        break;
                    case Cubeside.Back:
                        vertices1 = new[] {leftUpForward, rightUpForward, rightDownForward, leftDownForward};
                        triangles = new[] {1, 0, 3, 1, 3, 2,};
                        break;
                    case Cubeside.Up:
                        vertices1 = new[] {rightUpBack, leftUpBack, leftUpForward, rightUpForward};
                        triangles = new[] {0, 1, 2, 0, 2, 3,};
                        break;
                    case Cubeside.Down:
                        vertices1 = new[] {leftDownBack, rightDownBack, rightDownForward, leftDownForward};
                        triangles = new[] {0, 2, 3, 0, 1, 2};
                        break;
                    case Cubeside.Right:
                        vertices1 = new[] {rightDownBack, rightUpBack, rightUpForward, rightDownForward};
                        triangles = new[] {0, 1, 2, 0, 2, 3,};
                        break;
                    case Cubeside.Left:
                        vertices1 = new[] {leftDownBack, leftUpBack, leftUpForward, leftDownForward};
                        triangles = new[] {0, 3, 2, 0, 2, 1,};
                        break;

                    default:
                        continue;
                    // throw new ArgumentOutOfRangeException(nameof(side.Key), side.Key, null);
                }

                if (vertices1.Length == 0)
                    continue;
                var normales = new Vector3[]{side.Value,side.Value,side.Value,side.Value,};
                _voxelMeshDataPool[i].Vertices = vertices1.Select(v => v + positions.Value.ToVector3()).ToArray();
                _voxelMeshDataPool[i].Triangles = triangles;
                _voxelMeshDataPool[i].Normals = normales;
                i++;
            }
            var verts = _voxelMeshDataPool.SelectMany(v => v.Vertices).ToArray();
            var norms = _voxelMeshDataPool.SelectMany(v => v.Normals).ToArray();
            var tris = new List<int>();
            for (var j = 0; j < _voxelMeshDataPool.Length; j++)
            {
                tris.AddRange(_voxelMeshDataPool[j].Triangles.Select(triangle => triangle + (4 * j)));
            }
            return new Mesh()
            {
                vertices = verts,
                triangles = tris.ToArray(),
                normals = norms,
                uv = new Vector2[0]
            };
    }  
          private readonly Dictionary<Cubeside, Vector3> _sides = new Dictionary<Cubeside, Vector3>
          {
              {Cubeside.Back, Vector3.back},
              {Cubeside.Front, Vector3.forward},
              {Cubeside.Right, Vector3.right},
              {Cubeside.Left, Vector3.left},
              {Cubeside.Up, Vector3.up},
              {Cubeside.Down, Vector3.down}
          };
    }
    
    public enum Cubeside
    {
        Down,
        Up,
        Left,
        Right,
        Front,
        Back
    }
}

public static class Float3Ext
{
    public static Vector3 ToVector3(this float3 input)
    {
        return new Vector3(input.x, input.y, input.z);
    }
}