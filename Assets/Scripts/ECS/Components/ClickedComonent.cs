using Unity.Entities;

namespace ECS.Components
{
    public struct ClickedComponent : IComponentData
    {
        public int x;
        public int y;
    }
}
