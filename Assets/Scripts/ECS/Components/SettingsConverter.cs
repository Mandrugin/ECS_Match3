using System.Collections.Generic;
using ECS.Components.Spawn;
using Unity.Entities;
using UnityEngine;

namespace ECS.Components
{
    public class SettingsConverter : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public int Width;
        public int Height;
        public int Speed;
        public int MinGroupSize;
        
        // GemSet
        public GameObject[] Prefabs;
        
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.AddRange(Prefabs);
        }
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SettingsComponent
            {
                Width = Width,
                Height = Height,
                SetSize = Prefabs.Length,
                Speed = Speed,
                MinGroupSize = MinGroupSize
            });

            var buffer = dstManager.AddBuffer<GemSet>(entity);

            for (var i = 0; i < Prefabs.Length; ++i)
            {
                var prefab = conversionSystem.GetPrimaryEntity(Prefabs[i]);
                buffer.Add(new GemSet {Prefab = prefab});
            }
            
            dstManager.AddComponent<ScoreComponent>(entity);
        }
    }
}