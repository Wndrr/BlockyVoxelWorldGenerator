using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = System.Random;

namespace DefaultNamespace
{
    public class WorldSystem : JobComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public ComponentDataArray<ChunkComponentData> Chunks;
        }

        [Inject] private Data data;
        
        private struct WorldInitJob : IJobProcessComponentData<ChunkComponentData>
        {
            public ChunkComponentData Chunk;
            
            public void Execute(ref ChunkComponentData data)
            {
                data.Voxels = new VoxelData[8];
                for (var i = 0; i < data.Voxels.Length; i++)
                {
                    data.Voxels[i].Type = VoxelType.Stone;
                }

                data.NeedsRender = true;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            for (int i = 0; i < data.Chunks.Length; i++)
            {
                var job = new WorldInitJob()
                {
                    Chunk = data.Chunks[i]
                };
               job.Schedule(this, inputDeps);
            }
            
            return inputDeps;
        }
    }

    public class CreateChunkMesh : ComponentSystem 
    {
        struct Data
        {
            public readonly int Length;
            public RenderMesh RenderMesh;
        }
        
        protected override void OnUpdate()
        {
            var material = new Material(Shader.Find("Diffuse"));
            material.color = Color.magenta;
            var entities = GetEntities<Data>();
            GetEntit
            for (var index = 0; index < entities.Length; index++)
            {
                var entity = entities[index];
                entity.RenderMesh.material = material;
            }

            throw new System.NotImplementedException();
        }
    }
}