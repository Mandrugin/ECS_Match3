using Unity.Entities;
using UnityEngine;

namespace ECS.Components
{
    public class SettingsConverter : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Width;
        public int Height;
        // todo calculate size instead setting
        public int SetSize;
        public int Speed;
        public int MinGroupSize;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SettingsComponent
            {
                Width = Width,
                Height = Height,
                SetSize = SetSize,
                Speed = Speed,
                MinGroupSize = MinGroupSize
            });
        }
    }
}