using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerInputSystem : JobComponentSystem    
    {
        private  struct PlayerInputJob : IJobProcessComponentData<PlayerInput>
        {
            public float Horizontal;
            public void Execute(ref PlayerInput input)
            {
                input.Horizontal = Horizontal;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var job = new PlayerInputJob()
            {
                Horizontal = Input.GetAxis("Horizontal")
            };
            
            return job.Schedule(this, inputDeps);
        }
    }
}