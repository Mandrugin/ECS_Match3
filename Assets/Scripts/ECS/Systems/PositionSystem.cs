using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Systems
{
    [UpdateAfter(typeof(SpawnerSystem))]
    public class PositionSystem : JobComponentSystem
    {
        [BurstCompile]
        private struct PositionJob : IJobForEach<PositionComponent, Translation>
        {
            public float DeltaTime;
            public float Speed;

            public void Execute([ReadOnly] ref PositionComponent position, ref Translation translation)
            {
                var newPosition = new float3(position.x - 4.5f, position.y - 3.5f, 0);
                var direction = newPosition - translation.Value;
                if (math.length(direction) > 0.5f)
                {
                    var delta = math.normalize(direction) * Speed * DeltaTime;
                    translation.Value += delta;                    
                }
                else
                {
                    translation.Value = newPosition;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var setting = GetSingleton<SettingsComponent>();
            return new PositionJob{DeltaTime = Time.deltaTime, Speed = setting.Speed}.Schedule(this, inputDeps);
        }
    }
}