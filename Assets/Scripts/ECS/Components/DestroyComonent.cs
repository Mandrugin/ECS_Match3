using Unity.Entities;

namespace ECS.Components
{
    public struct DestroyComponent : IComponentData
    {
        public int x;
        public int y;
    }
}
