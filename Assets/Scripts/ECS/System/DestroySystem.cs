using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    public class DestroySystem : JobComponentSystem
    {
        private struct DestroyJob : IJobForEachWithEntity<PositionComponent>
        {
            [DeallocateOnJobCompletion]
            [ReadOnly]
            public NativeArray<DestroyComponent> DestroyComponents;
            public EntityCommandBuffer.Concurrent CommandBuffer;
            
            public void Execute(Entity entity, int index, [ReadOnly] ref PositionComponent positionComponent)
            {
                var count = DestroyComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    if (positionComponent.x == DestroyComponents[i].x
                        && positionComponent.y == DestroyComponents[i].y)
                    {
                        CommandBuffer.DestroyEntity(index, entity);
                    }
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
            var destroyJob = new DestroyJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer().ToConcurrent(),
                DestroyComponents = _destroyQuery.ToComponentDataArray<DestroyComponent>(Allocator.TempJob)
            };

            var jobHandle = destroyJob.Schedule(this, inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
