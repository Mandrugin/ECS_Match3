using ECS.Components;
using ECS.Components.Processing;
using ECS.Components.Spawn;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ECS.Systems
{
    public class SpawnerSystem : JobComponentSystem
    {
        private struct SpawnJob : IJob
        {
            public int Width; 
            public int Height;
            
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<int> RandomValues;
            [ReadOnly] public BufferFromEntity<GemSet> BufferFromEntity;
            public Entity SingletonEntity;
            public EntityCommandBuffer CommandBuffer;

            public void Execute()
            {
                var gemSet = BufferFromEntity[SingletonEntity];
            
                for (var x = 0; x < Width; ++x)
                {
                    for (var y = 0; y < Height; ++y)
                    {
                        var instance = CommandBuffer.Instantiate(gemSet[RandomValues[x * Height + y]].Prefab);

                        CommandBuffer.AddComponent(instance, new PositionComponent {x = x, y = y});
                        CommandBuffer.AddComponent(instance, new InGroupComponent());
                        CommandBuffer.AddComponent(instance, new GemTypeComponent{TypeId = RandomValues[x * Height + y]});
                    }
                }
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _positionQuery;

        protected override void OnCreate()
        {
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            _positionQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            RequireSingletonForUpdate<SettingsComponent>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_positionQuery.CalculateLength() > 0)
                return inputDeps;
            
            var settings = GetSingleton<SettingsComponent>();
            var singletonEntity = GetSingletonEntity<SettingsComponent>();

            var randomValues = new NativeArray<int>(settings.Width * settings.Height, Allocator.TempJob);

            for (var i = 0; i < settings.Width * settings.Height; ++i)
            {
                randomValues[i] = Random.Range(0, settings.SetSize);
            }
            
            var spawnJob = new SpawnJob
            {
                Width = settings.Width,
                Height = settings.Height,
                RandomValues = randomValues,
                BufferFromEntity = GetBufferFromEntity<GemSet>(true),
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                SingletonEntity = singletonEntity
            };

            var spawnHandler = spawnJob.Schedule(inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(spawnHandler);

            return spawnHandler;
        }
    }
}
