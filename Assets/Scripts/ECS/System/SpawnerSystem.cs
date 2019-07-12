using ECS.Components;
using ECS.Components.Processing;
using ECS.Components.Spawn;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace ECS.System
{
    public class SpawnerSystem : JobComponentSystem
    {
        private struct SpawnJob : IJob
        {
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<int> RandomValues;
            [ReadOnly] public BufferFromEntity<GemSet> BufferFromEntity;
            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Entity> Entities;
            public EntityCommandBuffer CommandBuffer;

            public void Execute()
            {
                for (var index = 0; index < Entities.Length; ++index)
                {
                    var gemSet = BufferFromEntity[Entities[index]];
                
                    for (var x = 0; x < 10; ++x)
                    {
                        for (var y = 0; y < 8; ++y)
                        {
                            var instance = CommandBuffer.Instantiate(gemSet[RandomValues[x * 8 + y]].Prefab);

                            CommandBuffer.AddComponent(instance, new PositionComponent {x = x, y = y});
                            CommandBuffer.AddComponent(instance, new InGroupComponent());
                            CommandBuffer.AddComponent(instance, new GemTypeComponent{TypeId = RandomValues[x * 8 + y]});
                        }
                    }

                    CommandBuffer.DestroyEntity(Entities[index]);                     
                }
            }
        }
        
        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _spawnerQuery;

        protected override void OnCreate()
        {
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            _spawnerQuery = GetEntityQuery(ComponentType.ReadOnly<GemSet>());
            RequireForUpdate(_spawnerQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var randomValues = new NativeArray<int>(10 * 8, Allocator.TempJob);

            for (var i = 0; i < 10 * 8; ++i)
            {
                randomValues[i] = Random.Range(0, 3);
            }
            
            var entities = _spawnerQuery.ToEntityArray(Allocator.TempJob);

            var spawnJob = new SpawnJob
            {
                RandomValues = randomValues,
                BufferFromEntity = GetBufferFromEntity<GemSet>(true),
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                Entities = entities
            };

            var spawnHandler = spawnJob.Schedule(inputDeps);
            
            _commandBuffer.AddJobHandleForProducer(spawnHandler);

            return spawnHandler;
        }
    }
}
