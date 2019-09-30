using ECS.Components;
using ECS.Components.Processing;
using ECS.Components.Spawn;
using ECS.Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ECS.Systems
{
    [UpdateAfter(typeof(MoveSystem))]
    public class SpawnerSystem : JobComponentSystem
    {
        private struct SpawnJob : IJob
        {
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<int> RandomValues;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Entity> CachedEntities;
            [ReadOnly] public BufferFromEntity<GemSet> GemSet;
            public Entity SingletonEntity;
            public EntityCommandBuffer CommandBuffer;
            public ArrayHelper Helper;

            public void Execute()
            {
                var gemSet = GemSet[SingletonEntity];

                for (var i = 0; i < CachedEntities.Length; ++i)
                {
                    if(CachedEntities[i] != Entity.Null) continue;

                    var entity = CommandBuffer.Instantiate(gemSet[RandomValues[i]].Prefab);
                    CommandBuffer.AddComponent(entity, new PositionComponent {x = Helper.GetX(i), y = Helper.GetY(i)});
                    CommandBuffer.AddComponent(entity, new InGroupComponent());
                    CommandBuffer.AddComponent(entity, new GemTypeComponent{TypeId = RandomValues[i]});
                    CommandBuffer.AddComponent(entity, new JustSpawned{Value = 1});
                }
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            RequireSingletonForUpdate<SettingsComponent>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var settings = GetSingleton<SettingsComponent>();
            var singletonEntity = GetSingletonEntity<SettingsComponent>();
            
            if (_positionsQuery.CalculateEntityCount() == settings.Width * settings.Height)
                return inputDeps;

            var helper = new ArrayHelper {Width = settings.Width, Height = settings.Height};
            
            var cachedEntities = new NativeArray<Entity>(settings.Width * settings.Height, Allocator.TempJob);
            var randomValues = new NativeArray<int>(settings.Width * settings.Height, Allocator.TempJob);

            for (var i = 0; i < settings.Width * settings.Height; ++i)
            {
                randomValues[i] = Random.Range(0, settings.SetSize);
            }
            
            var cacheJob = new CacheJob
            {
                CachedEntities = cachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob),
                Helper = helper
            };
            
            var spawnJob = new SpawnJob
            {
                CachedEntities = cachedEntities,
                RandomValues = randomValues,
                GemSet = GetBufferFromEntity<GemSet>(true),
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                SingletonEntity = singletonEntity,
                Helper = helper
            };

            var spawnHandler = cacheJob.Schedule(_positionsQuery.CalculateEntityCount(), 32, inputDeps);
            spawnHandler = spawnJob.Schedule(spawnHandler);
            
            _commandBuffer.AddJobHandleForProducer(spawnHandler);

            return spawnHandler;
        }
    }
}
