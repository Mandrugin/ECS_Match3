using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    [UpdateAfter(typeof(DestroySystem))]
    public class CleanupSystem : JobComponentSystem
    {
        private struct CleanJob : IJob
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<Entity> Entities;
            public EntityCommandBuffer CommandBuffer;

            public void Execute()
            {
                var length = Entities.Length;
                for (var i = 0; i < length; ++i)
                {
                    CommandBuffer.DestroyEntity(Entities[i]);
                }
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _destroyQuery;

        protected override void OnCreate()
        {
            _destroyQuery = GetEntityQuery(ComponentType.ReadOnly<DestroyComponent>());
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_destroyQuery);
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cleanJob = new CleanJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                Entities = _destroyQuery.ToEntityArray(Allocator.TempJob)
            };
            
            var jobHandle = cleanJob.Schedule(inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}