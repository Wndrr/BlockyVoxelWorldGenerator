using Components;
using Unity.Entities;
using Unity.Jobs;

namespace Systems
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
              //  data.Voxels = new VoxelData[8];
               // for (var i = 0; i < data.Voxels.Length; i++)
             //  {
             //      data.Voxels[i].Type = 0;
             //  }

                //data.NeedsRender = true;
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
               job.Schedule(this, inputDeps).Complete();
            }
            
            return inputDeps;
        }
    }
}