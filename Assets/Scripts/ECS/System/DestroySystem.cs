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
        
        // todo: move to separated system
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
            var destroyJob = new DestroyJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer().ToConcurrent(),
                DestroyComponents = _destroyQuery.ToComponentDataArray<DestroyComponent>(Allocator.TempJob)
            };

            var cleanJob = new CleanJob
            {
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                Entities = _destroyQuery.ToEntityArray(Allocator.TempJob)
            };

            var jobHandle = destroyJob.Schedule(this, inputDeps);
            jobHandle = cleanJob.Schedule(jobHandle);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
