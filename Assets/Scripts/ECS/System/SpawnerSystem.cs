using ECS.Components.Spawn;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.System
{
    public class SpawnerSystem : JobComponentSystem
    {
        private struct SpawnJob : IJobForEachWithEntity<Spawner, LocalToWorld>
        {
            public EntityCommandBuffer.Concurrent CommandBuffer;
            
            public void Execute(Entity entity, int index, [ReadOnly] ref Spawner spawner, [ReadOnly] ref LocalToWorld position)
            {
                for (var x = 0; x < 10; ++x)
                {
                    for (var y = 0; y < 8; ++y)
                    {
                        var instance = CommandBuffer.Instantiate(index, spawner.Prefab);

                        CommandBuffer.SetComponent(index, instance, new Translation{Value = new float3(x - 4.5f, y - 3.5f, 0)});
                    }
                }

                CommandBuffer.DestroyEntity(index, entity);
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;

        protected override void OnCreate()
        {
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var spawnJob = new SpawnJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer().ToConcurrent()
            };

            var spawnHandler = spawnJob.Schedule(this, inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(spawnHandler);

            return spawnHandler;
        }
    }
}
