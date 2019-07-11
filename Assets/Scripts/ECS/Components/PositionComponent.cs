using Unity.Entities;

namespace ECS.Components
{
    public struct PositionComponent : IComponentData
    {
        public int x;
        public int y;
    }
}
