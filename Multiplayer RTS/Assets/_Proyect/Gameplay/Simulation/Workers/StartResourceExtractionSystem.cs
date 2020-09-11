using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

//[DisableAutoCreation]
//public class StartResourceExtractionSystem : ComponentSystem
//{
//    public const int TICKS_FOR_RESOURCE_EXTRACTION = 30;
//    public Fix64  STOP_EXTRACTION_OFFSET = (Fix64)0.02;

//    protected override void OnUpdate()
//    {
//        Entities.WithNone<OnGatheringResources>().ForEach((Entity entity, ref HexPosition pos, ref ActionTarget target, ref ActionAttributes actAttributes, ref Gatherer gatherer) => 
//        {
//            if (EntityManager.HasComponent<ResourceSource>(target.TargetEntity))
//            {
//                var res = EntityManager.GetComponentData<ResourceSource>(target.TargetEntity);
//                if (target.TargetPosition.Distance(pos.HexCoordinates) <= actAttributes.ActRange)
//                {
//                    PostUpdateCommands.AddComponent<OnGatheringResources>(entity, new OnGatheringResources() 
//                    {
//                        gatheringResEntity = target.TargetEntity,
//                        gatheringResType = res.resourceType,

//                        maxCargo = gatherer.maxCargo,
//                        progressCount = 0,
//                        ticksNeededForExtraction = TICKS_FOR_RESOURCE_EXTRACTION
//                    });
//                }
//            }            
//        });


//        Entities.WithAll<OnGatheringResources>().ForEach((Entity entity, ref HexPosition pos, ref ActionTarget target, ref ActionAttributes actAttributes, ref Gatherer gatherer) =>
//        {

//        });
//    }
//}