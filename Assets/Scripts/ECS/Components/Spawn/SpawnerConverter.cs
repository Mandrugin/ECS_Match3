using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS.Components.Spawn
{
    public class SpawnerConverter : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject Prefab;
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(Prefab);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var spawner = new Spawner
            {
                Prefab = conversionSystem.GetPrimaryEntity(Prefab)
            };
            dstManager.AddComponentData(entity, spawner);
        }
    }
}
