using ECS.Components;
using ECS.Components.Processing;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    public class DestroySystem : JobComponentSystem
    {
        private struct CacheJob : IJobParallelFor
        {
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<Entity> CachedEntities;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<PositionComponent> Positions;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> Entities;
            
            public void Execute(int index)
            {
                var position = Positions[index];
                CachedEntities[position.y * 10 + position.x] = Entities[index];
            }
        }
        
        private struct DestroyJob : IJob
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> CachedEntities;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ClickedComponent> ClickedComponents;
            public EntityCommandBuffer CommandBuffer;

            public ComponentDataFromEntity<InGroupComponent> InGroup;
            [ReadOnly]
            public ComponentDataFromEntity<GemTypeComponent> GemType;
            
            public void Execute()
            {
                var count = ClickedComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    var destroyPos = ClickedComponents[i];
                    var clickedEntity = CachedEntities[destroyPos.y * 10 + destroyPos.x];
                    if(clickedEntity == Entity.Null) continue;
                    Analyse(destroyPos.y * 10 + destroyPos.x, GemType[clickedEntity].TypeId, 5);

                    for (var y = 0; y < CachedEntities.Length; ++y)
                    {
                        var entity = CachedEntities[y];
                        if (entity == Entity.Null) continue;
                        if (InGroup[entity].GroupId != 0)
                        {
                            CommandBuffer.DestroyEntity(entity);
                        }
                    }

                }
            }

            private void Analyse(int i, int typeId, int groupId)
            {
                if (i == -1) return;
                
                var entity = CachedEntities[i];

                // Check entity
                if (entity == Entity.Null) return;
                
                // Check type
                if (typeId != GemType[entity].TypeId) return;
                
                // Check group
                if (InGroup[entity].GroupId != 0) return;
                
                // Set group
                InGroup[entity] = new InGroupComponent {GroupId = groupId};
                
                Analyse(GetUp(i), typeId, groupId);
                Analyse(GetDown(i), typeId, groupId);
                Analyse(GetRight(i), typeId, groupId);
                Analyse(GetLeft(i), typeId, groupId);
            }

            private static int GetRight(int i)
            {
                if (i % 10 < 10 - 1) return i + 1;
                return -1;
            }
            
            private static int GetLeft(int i)
            {
                if (i % 10 > 0) return i - 1;
                return -1;
            }

            private static int GetUp(int i)
            {
                if ((i += 10) >= 10 * 8) return -1;
                return i;
            }

            private static int GetDown(int i)
            {
                if ((i -= 10) < 0) return -1;
                return i;                
            }
        }

        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _clickedQuery;
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _clickedQuery = GetEntityQuery(ComponentType.ReadOnly<ClickedComponent>());
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_clickedQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cachedEntities = new NativeArray<Entity>(10 * 8, Allocator.TempJob);

            var cacheJob = new CacheJob
            {
                CachedEntities = cachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob)
            };
            
            var destroyJob = new DestroyJob
            {
                CachedEntities = cachedEntities,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                ClickedComponents = _clickedQuery.ToComponentDataArray<ClickedComponent>(Allocator.TempJob),
                GemType = GetComponentDataFromEntity<GemTypeComponent>(true),
                InGroup = GetComponentDataFromEntity<InGroupComponent>(),
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateLength(), 32, inputDeps); 

            jobHandle = destroyJob.Schedule(jobHandle);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
