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
            public NativeArray<DestroyComponent> DestroyComponents;
            public EntityCommandBuffer CommandBuffer;

            public ComponentDataFromEntity<InGroupComponent> InGroup;
            [ReadOnly]
            public ComponentDataFromEntity<GemTypeComponent> GemType;
            
            public void Execute()
            {
                var count = DestroyComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    var destroyPos = DestroyComponents[i];
                    var destroyEntity = CachedEntities[destroyPos.y * 10 + destroyPos.x];
                    if(destroyEntity == Entity.Null) continue;
                    Analyse(destroyPos.y * 10 + destroyPos.x, GemType[destroyEntity].TypeId, 5);

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
                var entity = CachedEntities[i];

                // Check entity
                if (entity == Entity.Null)
                    return;
                
                // Check type
                if (typeId != GemType[entity].TypeId)
                {
                    return;
                }
                
                // Check group
                if (InGroup[entity].GroupId != 0)
                {
                    return;
                }
                
                // Set group
                InGroup[entity] = new InGroupComponent {GroupId = groupId};
                
                var up = GetUp(i);
                if (up != -1)
                {
                    Analyse(up, typeId, groupId);
                }

                var down = GetDown(i);
                if (down != -1)
                {
                    Analyse(down, typeId, groupId);
                }
                
                var right = GetRight(i);
                if (right != -1)
                {
                    Analyse(right, typeId, groupId);
                }
                
                var left = GetLeft(i);
                if (left != -1)
                {
                    Analyse(left, typeId, groupId);
                }
            }

            private int GetRight(int i)
            {
                var x = i % 10;
                var y = i / 10;
                
                x += 1;
                if (x >= 10)
                    return -1;
                return y * 10 + x;
            }
            
            private int GetLeft(int i)
            {
                var x = i % 10;
                var y = i / 10;
                
                x -= 1;
                if (x < 0)
                    return -1;
                return y * 10 + x;
            }

            private int GetUp(int i)
            {
                i += 10;
                if (i >= 10 * 8)
                    return -1;
                return i;
            }

            private int GetDown(int i)
            {
                i -= 10;
                if (i < 0)
                    return -1;
                return i;                
            }
        }

        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _destroyQuery;
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _destroyQuery = GetEntityQuery(ComponentType.ReadOnly<DestroyComponent>());
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_destroyQuery);
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
                DestroyComponents = _destroyQuery.ToComponentDataArray<DestroyComponent>(Allocator.TempJob),
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
