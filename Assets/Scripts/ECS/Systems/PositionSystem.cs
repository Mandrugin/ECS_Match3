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
        private struct PositionJob : IJobForEach<PositionComponent, JustSpawned, Translation>
        {
            public float DeltaTime;
            public float Speed;

            public int Height;

            public void Execute([ReadOnly] ref PositionComponent position, ref JustSpawned justSpawned, ref Translation translation)
            {
                var newPosition = new float3(position.x - 4.5f, position.y - 3.5f, 0);

                if (justSpawned.Value == 0)
                {
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
                else
                {
                    newPosition += new float3(0, Height, 0);
                    translation.Value = newPosition;
                    justSpawned.Value = 0;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var setting = GetSingleton<SettingsComponent>();
            return new PositionJob
            {
                DeltaTime = Time.deltaTime,
                Speed = setting.Speed,
                Height = setting.Height
            }.Schedule(this, inputDeps);
        }
    }
}