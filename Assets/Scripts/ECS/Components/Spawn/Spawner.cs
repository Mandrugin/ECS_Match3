using Unity.Entities;

namespace ECS.Components.Spawn
{
    public struct Spawner : IComponentData
    {
        public Entity Prefab;
    }
}
