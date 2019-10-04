using Unity.Entities;

namespace ECS.Components
{
    public struct ScoreComponent : IComponentData
    {
        public int Scores;
        public int OldScores;
    }
}
