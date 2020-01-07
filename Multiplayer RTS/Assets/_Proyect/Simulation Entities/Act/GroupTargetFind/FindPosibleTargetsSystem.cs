using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using System.Collections.Generic;


// FindPosibleTarget  System
//this system must read from the action attributes to see its act filters
//also it must read from the group AI current behaviour to see if it needs to find targets.
//using these filters and a scann distance it must update the possible target array buffer.

    //it also gets the group priority target in case if it needs it.(this may be better as another system)*

[DisableAutoCreation]
///acá se agrega el contenido del buffer y ademas se regula el componente "PriorityGroupTarget"
public class FindPosibleTargetsSystem : ComponentSystem
{    
    private EntityQuery m_PosibleTargetQuery;
    protected override void OnCreate()
    {
        m_PosibleTargetQuery = GetEntityQuery(typeof(HexPosition), typeof(Collider), typeof(Team)); //, typeof(Parent)
    }
    protected override void OnUpdate()
    {
        //THIS MUST READ THE USSER INPUT ON THE ENTITY BEFORE
        int count = m_PosibleTargetQuery.CalculateEntityCount();

        var positions        = new FractionalHex[count];
        var teams            = new int[count];
        var entities         = new Entity[count];
        var colliderLayers   = new ColliderLayer[count];
        var colliderRadiuses = new Fix64[count];

        var parentEntities = new Dictionary<Entity, List<int>>(count);

        int iteration = 0;
        Entities.ForEach((Entity entity, Parent parent, ref HexPosition position, ref Collider collider, ref Team team) =>//
        {
            positions[iteration]        = position.HexCoordinates;
            teams[iteration]            = team.Number;
            entities[iteration]         = entity;
            colliderLayers[iteration]   = collider.Layer;
            colliderRadiuses[iteration] = collider.Radious;

            if (parentEntities.TryGetValue(parent.ParentEntity, out List<int> childList))
            {
                childList.Add(iteration);
                parentEntities[parent.ParentEntity] = childList;
            }
            else
            {
                parentEntities.Add(parent.ParentEntity, new List<int>() { iteration });
            }

            iteration++;
        });
        var entitiesForeachHex = SpartialSortUtils.GetHexToPointsDictionary(positions);


        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref HexPosition hexPosition, ref SightRange sightRange, ref GroupBehaviour behaviour, ref ActTargetFilters filters, ref Team team, ref MovementState movementState) =>
        {
            var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            buffer.Clear();

            switch (behaviour.Value)
            {
                case Behaviour.PASSIVE:

                    if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
                    {
                        PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
                    }
                    if (!movementState.DestinationReached)
                    {
                        break;
                    }
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
                    break;

                case Behaviour.DEFAULT:

                    if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
                    {
                        PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
                    }
                    if (!movementState.DestinationReached)
                    {
                        break;
                    }
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
                    break;

                case Behaviour.AGRESIVE:
                    
                    PriorityGroupTarget priorityTarget;
                    if (GetPriorityTarget(out priorityTarget, hexPosition.HexCoordinates, sightRange.Value, filters, team, entities, teams, colliderLayers, entitiesForeachHex))
                    {
                        // 1° if there isn´t a priorityTarget component we add one.
                        if (!EntityManager.HasComponent<PriorityGroupTarget>(entity))
                        {
                            PostUpdateCommands.AddComponent<PriorityGroupTarget>(entity, priorityTarget);
                        }
                        else
                        {
                            PostUpdateCommands.SetComponent<PriorityGroupTarget>(entity, priorityTarget);
                        }


                        // 2° AddPriorityTargetSiblinsToBuffer
                        var targetEntity = priorityTarget.TargetEntity;
                        var targetPosition = priorityTarget.TargetPosition;
                        if (EntityManager.HasComponent<Parent>(targetEntity))
                        {
                            var parentComponent = EntityManager.GetSharedComponentData<Parent>(targetEntity);
                            List<int> childList;
                            Debug.Assert(parentEntities.TryGetValue(parentComponent.ParentEntity, out childList));

                            foreach (var targetIndex in childList)
                            {
                                buffer.Add(new BEPosibleTarget()
                                {
                                    Position = positions[targetIndex],
                                    Radius = colliderRadiuses[targetIndex],
                                    Entity = entities[targetIndex]
                                });
                            }
                        }
                        else
                        {
                            throw new System.NotImplementedException("the target selection in case of structures is not supported in the gresive system");
                        }


                    }
                    else
                    {
                        // 1° if there is a priorityTarget component we remove it.
                        if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
                        {
                            PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
                        }
                    }
                    break;
                    
                default:
                    throw new System.NotImplementedException();                    
            }

               
        });
    }
    /// <summary>
    /// Gets the target if true.
    /// debo ver una buena manera de conseguir el parent entity.
    /// </summary>
    private static bool GetPriorityTarget(out PriorityGroupTarget target, FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, Entity[] entities, int[] teams, ColliderLayer[] colliderLayers, Dictionary<Hex, List<SortPoint>> entitiesForeachHex)
    {        
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        target = new PriorityGroupTarget();
        var bestTargetDistance = Fix64.MaxValue;
        bool arePrioriryTarget = false;        

        //then we need to give priority towards especific targets. now is only the closest one.
        foreach (var point in pointsOfHex)
        {
            int pointTeam = teams[point.index];            
            var pointLayer = colliderLayers[point.index];
            var pointEntity = entities[point.index];

            bool validTarget = ValidPossibleTarget(position, sightRange, team.Number, point.position, pointLayer, filters, pointTeam);            
            if (validTarget)
            {
                //hay que ver si hay algo que bloquea el paso
                if (MapUtilities.PathToPointIsClear(position, point.position))
                {
                    arePrioriryTarget = true;
                    if (point.position.Distance(position) < bestTargetDistance)
                    {
                        target = new PriorityGroupTarget() { TargetPosition = point.position, TargetEntity = pointEntity, TargetHex = point.position.Round() };
                    }
                }                
            }
        }
        return arePrioriryTarget;
    }
    private static void FindAndAddPosibleTargetsToBuffer(FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, int[] teams, Entity[] entities, ColliderLayer[] colliderLayers, Fix64[] colliderRadiuses, Dictionary<Hex, List<SortPoint>> entitiesForeachHex, DynamicBuffer<BEPosibleTarget> buffer)
    {        
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        foreach (var point in pointsOfHex)
        {
            int pointTeam = teams[point.index];
            Entity pointEntity = entities[point.index];
            var pointLayer = colliderLayers[point.index];
            var pointRadius = colliderRadiuses[point.index];

            
            if (ValidPossibleTarget(position, sightRange, team.Number, point.position, pointLayer, filters, pointTeam))
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
    private static bool ValidPossibleTarget(FractionalHex position, Fix64 sightRange, int team, FractionalHex pointPos, ColliderLayer pointLayer, ActTargetFilters filters, int pointTeam)
    {
        if (pointPos.Distance(position) > sightRange || pointLayer == ColliderLayer.GROUP)
        {
            return false;
        }
        else 
        {
            return PassTeamFilter(filters, team, pointTeam);
        }
    }
    private static bool PassTeamFilter(ActTargetFilters filters ,int team, int teamB)
    {
        bool sameTeam = team == teamB;
        if (sameTeam && filters.ActOnTeamates)
        {
            return true;
        }
        else if (!sameTeam && filters.ActOnEnemies)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}