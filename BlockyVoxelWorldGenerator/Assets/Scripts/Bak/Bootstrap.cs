﻿using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

public class Bootstrap : MonoBehaviour
{
    public float Speed;
    public Mesh Mesh;
    public Material Material;
    
    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        var playerEntity = entityManager.CreateEntity(
            ComponentType.Create<Speed>(),
            ComponentType.Create<PlayerInput>(),
            ComponentType.Create<Position>(),
            ComponentType.Create<Transform>(),
            ComponentType.Create<RenderMesh >()
        );
        
        entityManager.SetComponentData(playerEntity, new Speed{Value = Speed});
        entityManager.SetSharedComponentData(playerEntity, new RenderMesh
        {
            mesh = Mesh,
            material = Material
        });
    }
}