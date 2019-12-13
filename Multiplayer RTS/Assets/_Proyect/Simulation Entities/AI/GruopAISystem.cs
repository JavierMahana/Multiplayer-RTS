using FixMath.NET;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class GruopAISystem : ComponentSystem
{        
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
            positions[iteration]        = position.HexCoordinates;
            teams[iteration]            = team.Number;
            entities[iteration]         = entity;
            colliderLayers[iteration]   = collider.Layer;
            colliderRadiuses[iteration] = collider.Radious;
            iteration++;
        });
        var entitiesForeachHex = SpartialSortUtils.GetHexToPointsDictionary(positions);

       
        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref GroupAI groupAI, ref HexPosition hexPosition, ref GroupBehaviour behaviour, ref Team team) => 
        {
            var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            buffer.Clear();

            var position = hexPosition.HexCoordinates;
            Hex roundedPosition = position.Round();
            var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(behaviour.SightDistance), entitiesForeachHex);

            bool triggered = false;
            foreach (var point in pointsOfHex)
            {
                int pointTeam      = teams[point.index];
                Entity pointEntity = entities[point.index];
                var pointLayer     = colliderLayers[point.index];
                var pointRadius    = colliderRadiuses[point.index];

                bool sameTeam = pointTeam == team.Number;

                bool clearPath = true;
                var map = MapManager.ActiveMap;
                Debug.Assert(map != null, "The Active Map is null!!!");
                var hexesInBewtween = Hex.HexesInBetween(position, point.position);
                foreach (Hex hex in hexesInBewtween)
                {
                    if (map.map.DinamicMapValues.TryGetValue(hex, out bool walkable))
                    {
                        if (!walkable)
                        {
                            clearPath = false;
                            break;
                        }
                    }
                    else 
                    {
                        clearPath = false;
                        break;
                    }
                }
                

                if (point.position.Distance(position) > behaviour.SightDistance || pointLayer == ColliderLayer.GROUP || !clearPath)
                {
                    continue;
                }
                

                if (sameTeam && behaviour.ActOnTeamates)
                {
                    triggered = true;
                    buffer.Add(new BEPosibleTarget() 
                    {
                        Position = point.position,
                        Radius = pointRadius,
                        Entity = pointEntity
                    });
                }
                else if (!sameTeam && behaviour.ActOnEnemies)
                {
                    triggered = true;
                    buffer.Add(new BEPosibleTarget() 
                    {
                        Position = point.position, 
                        Radius = pointRadius,
                        Entity = pointEntity
                    });
                }
            }

            groupAI.ArePossibleTargets = triggered;
        });

    }
}