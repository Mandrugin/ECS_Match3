using ECS.Components;
using ECS.Components.Processing;
using ECS.Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    [UpdateAfter(typeof(SplitSystem))]
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
            [ReadOnly]
            public ComponentDataFromEntity<InGroupComponent> InGroup;

            public Entity ScoreEntity;
            public ComponentDataFromEntity<ScoreComponent> Score;

            public ArrayHelper Helper;
            public int MinGroupSize;
            
            public void Execute()
            {
                var count = ClickedComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    var destroyPos = ClickedComponents[i];
                    var clickedEntity = CachedEntities[Helper.GetI(destroyPos.x, destroyPos.y)];
                    if(clickedEntity == Entity.Null) continue;
                    var groupId = InGroup[clickedEntity].GroupId;

                    var groupSize = 0;
                    
                    for (var y = 0; y < CachedEntities.Length; ++y)
                    {
                        var entity = CachedEntities[y];
                        if (entity == Entity.Null) continue;
                        if (InGroup[entity].GroupId == groupId)
                        {
                            ++groupSize;
                        }
                    }
                    
                    if(groupSize < MinGroupSize)
                        continue;

                    for (var y = 0; y < CachedEntities.Length; ++y)
                    {
                        var entity = CachedEntities[y];
                        if (entity == Entity.Null) continue;
                        if (InGroup[entity].GroupId == groupId)
                        {
                            CommandBuffer.DestroyEntity(entity);
                        }
                    }

                    var scores = Score[ScoreEntity];
                    Score[ScoreEntity] = new ScoreComponent {Scores = scores.Scores + groupSize};
                }
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
            var scoreEntity = GetSingletonEntity<ScoreComponent>();
            
            var cachedEntities = new NativeArray<Entity>(settings.Width * settings.Height, Allocator.TempJob);

            var helper = new ArrayHelper(settings.Width, settings.Height);

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
                InGroup = GetComponentDataFromEntity<InGroupComponent>(true),
                Score = GetComponentDataFromEntity<ScoreComponent>(),
                ScoreEntity = scoreEntity,
                Helper = helper,
                MinGroupSize = settings.MinGroupSize
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateEntityCount(), 32, inputDeps); 

            jobHandle = destroyJob.Schedule(jobHandle);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
