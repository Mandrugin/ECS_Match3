using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    [UpdateAfter(typeof(CacheSystem))]
    public class MoveSystem : JobComponentSystem
    {
        private struct MoveJob : IJob
        {
            public ComponentDataFromEntity<PositionComponent> Position;
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
        private CacheSystem _cacheSystem;

        protected override void OnCreate()
        {
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            _cacheSystem = World.GetOrCreateSystem<CacheSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_positionsQuery.CalculateLength() == 10 * 8) return inputDeps;
            
            var moveJob = new MoveJob
            {
                CachedEntities = _cacheSystem.CachedEntities,
                Position = GetComponentDataFromEntity<PositionComponent>(),
                Helper = new ArrayHelper {Width = 10, Height = 8}
            };

            return moveJob.Schedule(inputDeps);
        }
    }
}
