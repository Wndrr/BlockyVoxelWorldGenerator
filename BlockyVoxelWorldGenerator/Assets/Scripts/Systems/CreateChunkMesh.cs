using Unity.Entities;
using Unity.Rendering;
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
                renderMesh.material = material;
                EntityManager.SetSharedComponentData(entities[i], renderMesh);
            }
        }
    }
}