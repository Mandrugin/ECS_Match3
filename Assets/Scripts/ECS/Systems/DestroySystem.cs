using ECS.Components;
using ECS.Components.Processing;
using ECS.Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.Systems
{
    public struct ArrayHelper
    {
        public int Width;
        public int Height;
        
        public int GetRight(int i)
        {
            if (i % Width < Width - 1) return i + 1;
            return -1;
        }
            
        public int GetLeft(int i)
        {
            if (i % Width > 0) return i - 1;
            return -1;
        }

        public int GetUp(int i)
        {
            if ((i += Width) >= Width * Height) return -1;
            return i;
        }

        public int GetDown(int i)
        {
            if ((i -= Width) < 0) return -1;
            return i;
        }

        public int GetX(int i)
        {
            return i % Width;
        }

        public int GetY(int i)
        {
            return i / Width;
        }

        public int GetI(int x, int y)
        {
            return y * Width + x;
        }
    }
    
    public class DestroySystem : JobComponentSystem
    {
        private struct DestroyJob : IJob
        {
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> CachedEntities;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<ClickedComponent> ClickedComponents;
            public EntityCommandBuffer CommandBuffer;

            public ComponentDataFromEntity<InGroupComponent> InGroup;
            [ReadOnly]
            public ComponentDataFromEntity<GemTypeComponent> GemType;

            public ArrayHelper Helper;
            
            public void Execute()
            {
                var count = ClickedComponents.Length;
                for (var i = 0; i < count; ++i)
                {
                    var destroyPos = ClickedComponents[i];
                    var clickedEntity = CachedEntities[destroyPos.y * 10 + destroyPos.x];
                    if(clickedEntity == Entity.Null) continue;
                    Analyse(destroyPos.y * 10 + destroyPos.x, GemType[clickedEntity].TypeId, 5);

                    for (var y = 0; y < CachedEntities.Length; ++y)
                    {
                        var entity = CachedEntities[y];
                        if (entity == Entity.Null) continue;
                        if (InGroup[entity].GroupId != 0)
                        {
                            CommandBuffer.DestroyEntity(entity);
                        }
                    }

                }
            }

            private void Analyse(int i, int typeId, int groupId)
            {
                if (i == -1) return;
                
                var entity = CachedEntities[i];

                // Check entity
                if (entity == Entity.Null) return;
                
                // Check type
                if (typeId != GemType[entity].TypeId) return;
                
                // Check group
                if (InGroup[entity].GroupId != 0) return;
                
                // Set group
                InGroup[entity] = new InGroupComponent {GroupId = groupId};
                
                Analyse(Helper.GetUp(i), typeId, groupId);
                Analyse(Helper.GetDown(i), typeId, groupId);
                Analyse(Helper.GetRight(i), typeId, groupId);
                Analyse(Helper.GetLeft(i), typeId, groupId);
            }


        }

        private BeginInitializationEntityCommandBufferSystem _commandBuffer;
        private EntityQuery _clickedQuery;
        private EntityQuery _positionsQuery;

        protected override void OnCreate()
        {
            _clickedQuery = GetEntityQuery(ComponentType.ReadOnly<ClickedComponent>());
            _positionsQuery = GetEntityQuery(ComponentType.ReadOnly<PositionComponent>());
            _commandBuffer = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_clickedQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cachedEntities = new NativeArray<Entity>(10 * 8, Allocator.TempJob);

            var cacheJob = new CacheJob
            {
                CachedEntities = cachedEntities,
                Entities = _positionsQuery.ToEntityArray(Allocator.TempJob),
                Positions = _positionsQuery.ToComponentDataArray<PositionComponent>(Allocator.TempJob)
            };
            
            var destroyJob = new DestroyJob
            {
                CachedEntities = cachedEntities,
                CommandBuffer = _commandBuffer.CreateCommandBuffer(),
                ClickedComponents = _clickedQuery.ToComponentDataArray<ClickedComponent>(Allocator.TempJob),
                GemType = GetComponentDataFromEntity<GemTypeComponent>(true),
                InGroup = GetComponentDataFromEntity<InGroupComponent>(),
                Helper = new ArrayHelper{Width = 10, Height = 8}
            };

            var jobHandle = cacheJob.Schedule(_positionsQuery.CalculateLength(), 32, inputDeps); 

            jobHandle = destroyJob.Schedule(jobHandle);
            
            _commandBuffer.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}
