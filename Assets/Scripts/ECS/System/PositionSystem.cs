using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.System
{
    [UpdateAfter(typeof(SpawnerSystem))]
    public class PositionSystem : JobComponentSystem
    {
        private const float speed = 5f;
        
        [BurstCompile]
        private struct PositionJob : IJobForEach<PositionComponent, Translation>
        {
            public float DeltaTime;
            
            public void Execute([ReadOnly] ref PositionComponent position, ref Translation translation)
            {
                var newPosition = new float3(position.x - 4.5f, position.y - 3.5f, 0);
                var direction = newPosition - translation.Value;
                if (math.length(direction) > 0.1f)
                {
                    var delta = math.normalize(direction) * speed * DeltaTime;
                    translation.Value += math.normalize(direction) * speed * DeltaTime;                    
                }
                else
                {
                    translation.Value = newPosition;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new PositionJob{DeltaTime = Time.deltaTime}.Schedule(this, inputDeps);
        }
    }
}