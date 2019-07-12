using ECS.Components;
using ECS.Components.Processing;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    [UpdateAfter(typeof(DestroySystem))]
    public class CleanupSystem : JobComponentSystem
    {
        private struct CleanClicksJob : IJob
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

        private struct CleanInGroupsJob : IJobForEach<InGroupComponent>
        {
            public void Execute([WriteOnly] ref InGroupComponent inGroupComponent)
            {
                inGroupComponent.GroupId = 0;
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _clickedQuery;

        protected override void OnCreate()
        {
            _clickedQuery = GetEntityQuery(ComponentType.ReadOnly<ClickedComponent>());
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_clickedQuery);
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cleanClicksJob = new CleanClicksJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                Entities = _clickedQuery.ToEntityArray(Allocator.TempJob)
            };
            
            var clicksJobHandle = cleanClicksJob.Schedule(inputDeps);

            var cleanInGroupsJob = new CleanInGroupsJob();

            var cleanInGroupsJobHandle = cleanInGroupsJob.Schedule(this, inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(clicksJobHandle);

            return JobHandle.CombineDependencies(clicksJobHandle, cleanInGroupsJobHandle);
        }
    }
}