using System;
using ECS.Components;
using Unity.Entities;

namespace ECS.Systems
{
    [UpdateAfter(typeof(DestroySystem))]
    public class ScoreDisplaySystem : ComponentSystem
    {
        public event Action<int> ScoreChanged;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<ScoreComponent>();
        }

        protected override void OnUpdate()
        {
            var scoreComponent = GetSingleton<ScoreComponent>();
            if (scoreComponent.OldScores == scoreComponent.Scores)
                return;
            ScoreChanged?.Invoke(scoreComponent.Scores);
            scoreComponent.OldScores = scoreComponent.Scores;
            SetSingleton(scoreComponent);
        }
    }
}
