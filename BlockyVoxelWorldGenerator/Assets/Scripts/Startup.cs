using System;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;

public class Startup
{
    private static EntityManager _entityManager;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    public static EntityArchetype ChunkArchetype { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeScene()
    {
        _entityManager = World.Active.GetOrCreateManager<EntityManager>();
        CreateChunkArchetype();
    }

    private static void CreateChunkArchetype()
    {
        var positionComponent = ComponentType.Create<Position>();
        var localToWorldComponent = ComponentType.Create<LocalToWorld>();
        var renderMeshComponent = ComponentType.Create<RenderMesh>();
        var TransformComponent = ComponentType.Create<Transform>();
        var ChunkComponent = ComponentType.Create<ChunkComponentData>();

        ChunkArchetype = _entityManager.CreateArchetype(positionComponent, localToWorldComponent, renderMeshComponent, TransformComponent, ChunkComponent);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeAfterScene()
    {
        using (var chunks = new NativeArray<Entity>(4, Allocator.Temp))
        {
            _entityManager.CreateEntity(ChunkArchetype, chunks);

            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = gameObject.GetComponent<MeshFilter>().mesh;
            Object.Destroy(gameObject);
            for (var i = 0; i < chunks.Length; i++)
            {
                var material = new Material(Shader.Find("Diffuse"));
                _entityManager.SetSharedComponentData(chunks[i], new RenderMesh()
                {
                    mesh = mesh,
                    material = material
                });
                _entityManager.SetComponentData(chunks[i], new Position()
                {
                    Value = new float3(i, 0, 0)
                });
            }
        }
    }
}