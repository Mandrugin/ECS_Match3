using ECS.Components;
using ECS.Components.Processing;
using ECS.Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    public class DestroySystem : JobComponentSystem
    {
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

            public ArrayHelper Helper;
            
            public void Execute()
            {
                var count = ClickedComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    var destroyPos = ClickedComponents[i];
                    var clickedEntity = CachedEntities[destroyPos.y * Helper.Width + destroyPos.x];
                    if(clickedEntity == Entity.Null) continue;
                    Analyse(destroyPos.y * Helper.Width + destroyPos.x, GemType[clickedEntity].TypeId, 5);

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
                
                Analyse(Helper.GetUp(i), typeId, groupId);
                Analyse(Helper.GetDown(i), typeId, groupId);
                Analyse(Helper.GetRight(i), typeId, groupId);
                Analyse(Helper.GetLeft(i), typeId, groupId);
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
            var settings = GetSingleton<SettingsComponent>();
            
            var cachedEntities = new NativeArray<Entity>(settings.Width * settings.Height, Allocator.TempJob);

            var helper = new ArrayHelper {Width = settings.Width, Height = settings.Height};

            var cacheJob = new CacheJob
            {
                CachedEntities = cachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob),
                Helper = helper
            };
            
            var destroyJob = new DestroyJob
            {
                CachedEntities = cachedEntities,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                ClickedComponents = _clickedQuery.ToComponentDataArray<ClickedComponent>(Allocator.TempJob),
                GemType = GetComponentDataFromEntity<GemTypeComponent>(true),
                InGroup = GetComponentDataFromEntity<InGroupComponent>(),
                Helper = helper
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateLength(), 32, inputDeps); 

            jobHandle = destroyJob.Schedule(jobHandle);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
