using Unity.Entities;

namespace ECS.Components.Processing
{
    public struct InGroupComponent : IComponentData
    {
        public int GroupId;
    }
}
