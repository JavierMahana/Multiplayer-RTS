using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

//public class FindGroupPriorityTargetSystem : ComponentSystem
//{    
//    private EntityQuery m_ColliderQuery;
//    protected override void OnCreate()
//    {
//        m_ColliderQuery = GetEntityQuery(typeof(HexPosition), typeof(Collider), typeof(Team));
//    }
//    protected override void OnUpdate()
//    {
//        int count = m_ColliderQuery.CalculateEntityCount();

//        var positions = new FractionalHex[count];
//        var teams = new int[count];
//        var entities = new Entity[count];
//        var colliderLayers = new ColliderLayer[count];
//        var colliderRadiuses = new Fix64[count];

//        int iteration = 0;
//        Entities.ForEach((Entity entity, ref HexPosition position, ref Collider collider, ref Team team) =>
//        {
//            positions[iteration] = position.HexCoordinates;
//            teams[iteration] = team.Number;
//            entities[iteration] = entity;
//            colliderLayers[iteration] = collider.Layer;
//            colliderRadiuses[iteration] = collider.Radious;
//            iteration++;
//        });


//        Entities.WithAll<Group, PriorityGroupTarget>().ForEach(
//        (ref HexPosition hexPosition, ref ActTargetFilters targetFilters, ref GroupBehaviour behaviour, ref Team team) => 
//        {

//        });
//        Entities.WithAll<Group>().WithNone<PriorityGroupTarget>().ForEach(
//        (ref HexPosition hexPosition, ref ActTargetFilters targetFilters, ref GroupBehaviour behaviour, ref Team team) =>
//        {

//        });
//    }
//}