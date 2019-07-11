using Unity.Entities;

namespace ECS.Components.Spawn
{
    [InternalBufferCapacity(3)]
    public struct GemSet : IBufferElementData
    {
        public Entity Prefab;
    }
}
