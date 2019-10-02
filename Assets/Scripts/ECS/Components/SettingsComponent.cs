using Unity.Entities;

namespace ECS.Components
{
    public struct SettingsComponent : IComponentData
    {
        public int Width;
        public int Height;
        public int SetSize;
        public int Speed;
        public int MinGroupSize;
    }
}
