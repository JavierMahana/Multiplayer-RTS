using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using static Unity.Mathematics.math;
using FixMath.NET;
using UnityEngine;

[DisableAutoCreation]
public class FindMovementTargetSystem : ComponentSystem
{
    private readonly static Fix64 TARGET_OFFSET_FROM_GROUP_CENTER = (Fix64)0.4;

    protected override void OnUpdate()
    {
        List<FractionalHex> groupPosition = new List<FractionalHex>();
        List<FractionalHex> groupDirection = new List<FractionalHex>();
        Dictionary<Entity, int> groupIndices = new Dictionary<Entity, int>();

        int groupIndex = 0;
        Entities.WithAll<Group>().ForEach((Entity entity, ref HexPosition position, ref DirectionAverage direction) =>
        {
            groupPosition.Add(position.HexCoordinates);
            groupDirection.Add(direction.Value);
            groupIndices.Add(entity, groupIndex);
            groupIndex++;
        });

        Entities.WithAll<OnGroup>().ForEach((Parent parent, ref SteeringTarget target) =>
        {

            if (!groupIndices.TryGetValue(parent.ParentEntity, out int parentGroupIndex))
            {
                Debug.LogError($"This entity: {parent.ParentEntity} is a parent and it needs to have a Group, hexPosition, and a directionAverage component");
                return;
            }
            var parentPos = groupPosition[parentGroupIndex];
            var parentDirection = groupDirection[parentGroupIndex];


            target.TargetPosition = parentPos + parentDirection * TARGET_OFFSET_FROM_GROUP_CENTER;


            if (parentDirection == FractionalHex.Zero)
                target.StopAtTarget = true;
            else
                target.StopAtTarget = false;
        });



        Entities.WithAll<PathWaypoint>().WithAny<OnReinforcement, Group>().ForEach(
        (Entity entity, ref HexPosition position, ref SteeringTarget target, ref PathWaypointIndex waypointIndex) =>
        {
            var buffer = EntityManager.GetBuffer<PathWaypoint>(entity);

            if (waypointIndex.Value >= buffer.Length)
            {
                Debug.Log("There are not waypoints for the current index");
                target.TargetPosition = position.HexCoordinates;
                return;
            }

            if (waypointIndex.Value == buffer.Length - 1)
                target.StopAtTarget = true;
            else
                target.StopAtTarget = false;

            target.TargetPosition = (FractionalHex)buffer[waypointIndex.Value].Value;
        });
    }
}