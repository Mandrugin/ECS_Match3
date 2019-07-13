using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS.Components.Spawn
{
    public class GemSetConverter : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject[] Prefabs;
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            for (var i = 0; i < Prefabs.Length; ++i)
            {
                referencedPrefabs.Add(Prefabs[i]);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<GemSet>(entity);

            for (var i = 0; i < Prefabs.Length; ++i)
            {
                var prefab = conversionSystem.GetPrimaryEntity(Prefabs[i]);
                buffer.Add(new GemSet {Prefab = prefab});
            }
        }
    }
}
