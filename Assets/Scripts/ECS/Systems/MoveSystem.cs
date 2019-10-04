﻿using ECS.Components;
using ECS.Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    // todo implement cache system
    public class MoveSystem : JobComponentSystem
    {
        private struct MoveJob : IJob
        {
            public ComponentDataFromEntity<PositionComponent> Position;
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> CachedEntities;

            public ArrayHelper Helper;
            
            public void Execute()
            {
                for (var x = 0; x < Helper.Width; ++x)
                {
                    var y = 0;
                    while (y < Helper.Height - 1)
                    {
                        var currIndex = Helper.GetI(x, y);
                        var currEntity = CachedEntities[currIndex];
                        if (currEntity != Entity.Null)
                        {
                            y += 1;
                            continue;
                        }

                        var topY = y + 1;
                        while (topY < Helper.Height)
                        {
                            var topIndex = Helper.GetI(x, topY);
                            var topEntity = CachedEntities[topIndex];

                            if (topEntity == Entity.Null)
                            {
                                topY += 1;
                                continue;
                            }

                            Position[topEntity] = new PositionComponent{x = x, y = y};
                            CachedEntities[topIndex] = Entity.Null;
                            CachedEntities[currIndex] = topEntity;
                            break;
                        }
                        y += 1;
                    }
                }
            }
        }
        
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var settings = GetSingleton<SettingsComponent>();
            
            if (_positionsQuery.CalculateEntityCount() == settings.Width * settings.Height) return inputDeps;

            var helper = new ArrayHelper(settings.Width, settings.Height);
            
            var cachedEntities = new NativeArray<Entity>(settings.Width * settings.Height, Allocator.TempJob);

            var cacheJob = new CacheJob
            {
                CachedEntities = cachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob),
                Helper = helper
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateEntityCount(), 32, inputDeps);

            var moveJob = new MoveJob
            {
                CachedEntities = cachedEntities,
                Position = GetComponentDataFromEntity<PositionComponent>(),
                Helper = helper
            };

            jobHandle = moveJob.Schedule(jobHandle);

            return jobHandle;
        }
    }
}
