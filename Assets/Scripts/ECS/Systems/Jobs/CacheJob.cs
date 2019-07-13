using ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems.Jobs
{
    public struct CacheJob : IJobParallelFor
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

        public int Width;
            
        public void Execute(int index)
        {
            var position = Positions[index];
            CachedEntities[position.y * Width + position.x] = Entities[index];
        }
    }
}