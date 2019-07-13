using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    public class CacheSystem : JobComponentSystem
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
        
        public NativeArray<Entity> CachedEntities;
        
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            CachedEntities = new NativeArray<Entity>(10 * 8, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            CachedEntities.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var cacheJob = new CacheJob
            {
                CachedEntities = CachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob)
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateLength(), 32, inputDeps);

            return jobHandle;
        }
    }
}