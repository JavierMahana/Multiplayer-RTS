using FixMath.NET;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


//[DisableAutoCreation]
//public class FindSteeringTargetSystem : ComponentSystem
//{
//    private readonly static Fix64 TARGET_OFFSET_FROM_GROUP_CENTER = (Fix64)0.5;

//    protected override void OnUpdate()
//    {
//        List<FractionalHex> groupPosition = new List<FractionalHex>();
//        List<FractionalHex> groupDirection = new List<FractionalHex>();
//        Dictionary<Entity, int> groupIndices = new Dictionary<Entity, int>();

//        int groupIndex = 0;
//        Entities.WithAll<Group>().ForEach((Entity entity, ref HexPosition position, ref DirectionAverage direction) =>
//        {
//            groupPosition.Add(position.HexCoordinates);
//            groupDirection.Add(direction.Value);
//            groupIndices.Add(entity, groupIndex);
//            groupIndex++;
//        });

//        Entities.ForEach((ref InGroup group, ref SteeringTarget target) =>
//        {
//            if (!groupIndices.TryGetValue(group.groupEntity, out int groupIndex))
//            {
//                return;
//            }
//            var parentPos = groupPosition[groupIndex];
//            var parentDirection = groupDirection[groupIndex];
//            target = new SteeringTarget() { TargetPosition = parentPos + (parentDirection * TARGET_OFFSET_FROM_GROUP_CENTER) };
//        });


//        Entities.WithNone<InGroup>().WithAll<PathWaypoint>().ForEach((Entity entity, ref PathWaypointIndex waypointIndex, ref SteeringTarget target) => 
//        {
//            var buffer = EntityManager.GetBuffer<PathWaypoint>(entity);
//            if (buffer.Length <= waypointIndex.Value)
//            {
                
//            }
//        });
//    }
//}
