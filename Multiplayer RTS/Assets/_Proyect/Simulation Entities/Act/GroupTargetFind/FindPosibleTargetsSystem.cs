using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;


// FindPosibleTarget  System
//this system must read from the action attributes to see its act filters
//also it must read from the group AI current behaviour to see if it needs to find targets.
//using these filters and a scann distance it must update the possible target array buffer.

[DisableAutoCreation]
public class FindPosibleTargetsSystem : ComponentSystem
{

    private static readonly Fix64 GroupsTargetSight = (Fix64)2;
    private EntityQuery m_ColliderQuery;
    protected override void OnCreate()
    {
        m_ColliderQuery = GetEntityQuery(typeof(HexPosition), typeof(Collider), typeof(Team));
    }
    protected override void OnUpdate()
    {
        //THIS MUST READ THE USSER INPUT ON THE ENTITY BEFORE
        int count = m_ColliderQuery.CalculateEntityCount();

        var positions        = new FractionalHex[count];
        var teams            = new int[count];
        var entities         = new Entity[count];
        var colliderLayers   = new ColliderLayer[count];
        var colliderRadiuses = new Fix64[count];

        int iteration = 0;
        Entities.ForEach((Entity entity, ref HexPosition position, ref Collider collider, ref Team team) =>
        {
            positions[iteration] = position.HexCoordinates;
            teams[iteration] = team.Number;
            entities[iteration] = entity;
            colliderLayers[iteration] = collider.Layer;
            colliderRadiuses[iteration] = collider.Radious;
            iteration++;
        });
        var entitiesForeachHex = SpartialSortUtils.GetHexToPointsDictionary(positions);


        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref HexPosition hexPosition, ref GroupBehaviour behaviour, ref ActTargetFilters filters, ref Team team, ref MovementState movementState) =>
        {
            var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            buffer.Clear();

            switch (behaviour.Value)
            {
                case Behaviour.PASSIVE:

                    if (!movementState.DestinationReached)
                    {
                        break;
                    }
                    FindAndAddPosibleTargetsToBuffer(hexPosition, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
                    break;

                case Behaviour.DEFAULT:

                    if (!movementState.DestinationReached)
                    {
                        break;
                    }
                    FindAndAddPosibleTargetsToBuffer(hexPosition, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
                    break;

                case Behaviour.AGRESIVE:
                    throw new System.NotImplementedException();
                    
                default:
                    throw new System.NotImplementedException();                    
            }

               
        });
    }

    private static void FindAndAddPosibleTargetsToBuffer(HexPosition hexPosition, ActTargetFilters filters, Team team, int[] teams, Entity[] entities, ColliderLayer[] colliderLayers, Fix64[] colliderRadiuses, System.Collections.Generic.Dictionary<Hex, System.Collections.Generic.List<SortPoint>> entitiesForeachHex, DynamicBuffer<BEPosibleTarget> buffer)
    {
        var position = hexPosition.HexCoordinates;
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(GroupsTargetSight), entitiesForeachHex);

        foreach (var point in pointsOfHex)
        {
            int pointTeam = teams[point.index];
            Entity pointEntity = entities[point.index];
            var pointLayer = colliderLayers[point.index];
            var pointRadius = colliderRadiuses[point.index];

            //bool clearPath = PathToPointIsClear(position, point); moved to find action system
            if (point.position.Distance(position) > GroupsTargetSight || pointLayer == ColliderLayer.GROUP)//|| !clearPath)
            {
                continue;
            }


            bool sameTeam = pointTeam == team.Number;
            if (sameTeam && filters.ActOnTeamates)
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = point.position,
                    Radius = pointRadius,
                    Entity = pointEntity
                });
            }
            else if (!sameTeam && filters.ActOnEnemies)
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = point.position,
                    Radius = pointRadius,
                    Entity = pointEntity
                });
            }
        }
    }
}