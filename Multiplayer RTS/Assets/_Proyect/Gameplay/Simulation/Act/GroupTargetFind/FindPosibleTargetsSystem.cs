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
///es contenido de la entidad padre o grupal.
public class FindPosibleTargetsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        List<EntityOnVision> entitiesOnVisionTeam_0 = SightSystem.GetEntitiesOnVisionOfTeam(0);
        List<EntityOnVision> entitiesOnVisionTeam_1 = SightSystem.GetEntitiesOnVisionOfTeam(1);
        List<EntityOnVision> entitiesOnVisionTeam_2 = SightSystem.GetEntitiesOnVisionOfTeam(2);
        List<EntityOnVision> entitiesOnVisionTeam_3 = SightSystem.GetEntitiesOnVisionOfTeam(3);
        List<EntityOnVision> entitiesOnVisionTeam_4 = SightSystem.GetEntitiesOnVisionOfTeam(4);
        List<EntityOnVision> entitiesOnVisionTeam_5 = SightSystem.GetEntitiesOnVisionOfTeam(5);
        List<EntityOnVision> entitiesOnVisionTeam_6 = SightSystem.GetEntitiesOnVisionOfTeam(6);
        List<EntityOnVision> entitiesOnVisionTeam_7 = SightSystem.GetEntitiesOnVisionOfTeam(7);


        var sortedByHexEntitiesOnVisionTeam_0 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_0);
        var sortedByHexEntitiesOnVisionTeam_1 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_1);
        var sortedByHexEntitiesOnVisionTeam_2 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_2);
        var sortedByHexEntitiesOnVisionTeam_3 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_3);
        var sortedByHexEntitiesOnVisionTeam_4 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_4);
        var sortedByHexEntitiesOnVisionTeam_5 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_5);
        var sortedByHexEntitiesOnVisionTeam_6 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_6);
        var sortedByHexEntitiesOnVisionTeam_7 = SpartialSortUtils.GetHexToPointsDictionary(entitiesOnVisionTeam_7);



        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref HexPosition hexPosition, ref SightRange sightRange, ref GroupBehaviour behaviour, ref ActTargetFilters filters, ref Team team, ref MovementState movementState) =>
        {
            var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            buffer.Clear();
            var sortedEntitiesOnTeamVision = GetSortedEntitiesOnTeamVision(team.Number, 
                sortedByHexEntitiesOnVisionTeam_0, sortedByHexEntitiesOnVisionTeam_1, sortedByHexEntitiesOnVisionTeam_2, sortedByHexEntitiesOnVisionTeam_3,
                sortedByHexEntitiesOnVisionTeam_4, sortedByHexEntitiesOnVisionTeam_5, sortedByHexEntitiesOnVisionTeam_6, sortedByHexEntitiesOnVisionTeam_7);

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
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, sortedEntitiesOnTeamVision, buffer);
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
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, sortedEntitiesOnTeamVision, buffer);
                    break;


                    
                case Behaviour.AGRESIVE://este modo agrega la entidad más cercana no bloqueada, como blanco prioritario. agrega a todas las entidades cercanas como posibles targets.
                    
                    PriorityGroupTarget priorityTarget;
                    if (GetPriorityTargetAndAddPosibleTargets(out priorityTarget, hexPosition.HexCoordinates, sightRange.Value, team, filters, sortedEntitiesOnTeamVision,buffer))
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

    private static Dictionary<Hex, List<EntityOnVision>> GetSortedEntitiesOnTeamVision(int team, Dictionary<Hex, List<EntityOnVision>> EOVTeam_0,
        Dictionary<Hex, List<EntityOnVision>> EOVTeam_1, Dictionary<Hex, List<EntityOnVision>> EOVTeam_2, Dictionary<Hex, List<EntityOnVision>> EOVTeam_3,
        Dictionary<Hex, List<EntityOnVision>> EOVTeam_4, Dictionary<Hex, List<EntityOnVision>> EOVTeam_5, Dictionary<Hex, List<EntityOnVision>> EOVTeam_6,
        Dictionary<Hex, List<EntityOnVision>> EOVTeam_7)
    {
        switch (team)
        {
            case 0:
                return EOVTeam_0;
            case 1:
                return EOVTeam_1;
            case 2:
                return EOVTeam_2;
            case 3:
                return EOVTeam_3;
            case 4:
                return EOVTeam_4;
            case 5:
                return EOVTeam_5;
            case 6:
                return EOVTeam_6;
            case 7:
                return EOVTeam_7;
            default:
                throw new System.ArgumentException("The team must be in the 0-7 range. other options are not implementedS");

        }
    }
    /// <summary>
    /// Gets the target if true.
    /// debo ver una buena manera de conseguir el parent entity.
    /// </summary>
    private static bool GetPriorityTargetAndAddPosibleTargets(out PriorityGroupTarget target, FractionalHex position, Fix64 sightRange, Team team, ActTargetFilters filters, Dictionary<Hex, List<EntityOnVision>> entitiesForeachHex, DynamicBuffer<BEPosibleTarget> buffer)
    {
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        target = new PriorityGroupTarget();
        var bestTargetDistance = Fix64.MaxValue;
        bool arePrioriryTarget = false;

        foreach (var point in pointsOfHex)
        {
            int pointTeam = point.team;
            Entity pointEntity = point.entity;
            var pointLayer = point.collider.Layer;
            var pointRadius = point.collider.Radius;


            if (ValidPossibleTarget(position, sightRange, team.Number, filters, point.position, pointRadius, pointLayer, pointTeam))
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = point.position,
                    Radius = pointRadius,
                    Entity = pointEntity
                });
                //hay que ver si hay algo que bloquea el paso
                if (MapUtilities.PathToPointIsClear(position, point.position))
                {
                    arePrioriryTarget = true;
                    var distance = point.position.Distance(position);
                    if (distance < bestTargetDistance)
                    {
                        bestTargetDistance = distance;
                        target = new PriorityGroupTarget() { TargetPosition = point.position, TargetEntity = pointEntity, TargetHex = point.position.Round() };
                    }
                }
            }
        }
        return arePrioriryTarget;
    }
    private static void FindAndAddPosibleTargetsToBuffer(FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, Dictionary<Hex, List<EntityOnVision>> entitiesForeachHex, DynamicBuffer<BEPosibleTarget> buffer)
    {        
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        foreach (var point in pointsOfHex)
        {
            int pointTeam = point.team;
            Entity pointEntity = point.entity;
            var pointLayer = point.collider.Layer;
            var pointRadius = point.collider.Radius;

            
            if (ValidPossibleTarget(position, sightRange, team.Number, filters, point.position, pointRadius, pointLayer , pointTeam))
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
    private static bool ValidPossibleTarget(FractionalHex position, Fix64 sightRange, int team, ActTargetFilters filters, FractionalHex pointPos, Fix64 pointRadius, ColliderLayer pointLayer, int pointTeam)
    {
        Debug.Assert(pointLayer != ColliderLayer.GROUP, "Hay un error en el sistema de vision. Dejo pasar una entidad de grupo.");
        if (pointPos.Distance(position) > sightRange + pointRadius)
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


//private EntityQuery m_PosibleTargetQuery;
//protected override void OnCreate()
//{
//    m_PosibleTargetQuery = GetEntityQuery(typeof(HexPosition), typeof(Collider), typeof(Team)); //, typeof(Parent)
//}
//protected override void OnUpdate()
//{
//    //THIS MUST READ THE USSER INPUT ON THE ENTITY BEFORE
//    int count = m_PosibleTargetQuery.CalculateEntityCount();

//    var positions = new FractionalHex[count];
//    var teams = new int[count];
//    var entities = new Entity[count];
//    var colliderLayers = new ColliderLayer[count];
//    var colliderRadiuses = new Fix64[count];

//    var parentEntities = new Dictionary<Entity, List<int>>(count);

//    int iteration = 0;
//    Entities.ForEach((Entity entity, Parent parent, ref HexPosition position, ref Collider collider, ref Team team) =>//
//    {
//        positions[iteration] = position.HexCoordinates;
//        teams[iteration] = team.Number;
//        entities[iteration] = entity;
//        colliderLayers[iteration] = collider.Layer;
//        colliderRadiuses[iteration] = collider.Radius;

//        if (parentEntities.TryGetValue(parent.ParentEntity, out List<int> childList))
//        {
//            childList.Add(iteration);
//            parentEntities[parent.ParentEntity] = childList;
//        }
//        else
//        {
//            parentEntities.Add(parent.ParentEntity, new List<int>() { iteration });
//        }

//        iteration++;
//    });
//    var entitiesForeachHex = SpartialSortUtils.GetHexToPointsDictionary(positions);


//    Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref HexPosition hexPosition, ref SightRange sightRange, ref GroupBehaviour behaviour, ref ActTargetFilters filters, ref Team team, ref MovementState movementState) =>
//    {
//        var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
//        buffer.Clear();

//        switch (behaviour.Value)
//        {
//            case Behaviour.PASSIVE:

//                if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
//                {
//                    PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
//                }
//                if (!movementState.DestinationReached)
//                {
//                    break;
//                }
//                FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
//                break;

//            case Behaviour.DEFAULT:

//                if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
//                {
//                    PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
//                }
//                if (!movementState.DestinationReached)
//                {
//                    break;
//                }
//                FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, teams, entities, colliderLayers, colliderRadiuses, entitiesForeachHex, buffer);
//                break;

//            case Behaviour.AGRESIVE:

//                PriorityGroupTarget priorityTarget;
//                if (GetPriorityTarget(out priorityTarget, hexPosition.HexCoordinates, sightRange.Value, filters, team, entities, teams, colliderLayers, entitiesForeachHex))
//                {
//                    // 1° if there isn´t a priorityTarget component we add one.
//                    if (!EntityManager.HasComponent<PriorityGroupTarget>(entity))
//                    {
//                        PostUpdateCommands.AddComponent<PriorityGroupTarget>(entity, priorityTarget);
//                    }
//                    else
//                    {
//                        PostUpdateCommands.SetComponent<PriorityGroupTarget>(entity, priorityTarget);
//                    }


//                    // 2° AddPriorityTargetSiblinsToBuffer
//                    var targetEntity = priorityTarget.TargetEntity;
//                    var targetPosition = priorityTarget.TargetPosition;
//                    if (EntityManager.HasComponent<Parent>(targetEntity))
//                    {
//                        var parentComponent = EntityManager.GetSharedComponentData<Parent>(targetEntity);
//                        List<int> childList;
//                        Debug.Assert(parentEntities.TryGetValue(parentComponent.ParentEntity, out childList));

//                        foreach (var targetIndex in childList)
//                        {
//                            buffer.Add(new BEPosibleTarget()
//                            {
//                                Position = positions[targetIndex],
//                                Radius = colliderRadiuses[targetIndex],
//                                Entity = entities[targetIndex]
//                            });
//                        }
//                    }
//                    else
//                    {
//                        throw new System.NotImplementedException("the target selection in case of structures is not supported in the agresive system");
//                    }


//                }
//                else
//                {
//                    // 1° if there is a priorityTarget component we remove it.
//                    if (EntityManager.HasComponent<PriorityGroupTarget>(entity))
//                    {
//                        PostUpdateCommands.RemoveComponent<PriorityGroupTarget>(entity);
//                    }
//                }
//                break;

//            default:
//                throw new System.NotImplementedException();
//        }


//    });
//}
/// <summary>
/// Gets the target if true.
/// debo ver una buena manera de conseguir el parent entity.
/// </summary>
//private static bool GetPriorityTarget(out PriorityGroupTarget target, FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, Entity[] entities, int[] teams, ColliderLayer[] colliderLayers, Dictionary<Hex, List<SortPoint>> entitiesForeachHex)
//{
//    Hex roundedPosition = position.Round();
//    var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

//    target = new PriorityGroupTarget();
//    var bestTargetDistance = Fix64.MaxValue;
//    bool arePrioriryTarget = false;

//    //then we need to give priority towards especific targets. now is only the closest one.
//    foreach (var point in pointsOfHex)
//    {
//        int pointTeam = teams[point.index];
//        var pointLayer = colliderLayers[point.index];
//        var pointEntity = entities[point.index];

//        bool validTarget = ValidPossibleTarget(position, sightRange, team.Number, point.position, pointLayer, filters, pointTeam);
//        if (validTarget)
//        {
//            //hay que ver si hay algo que bloquea el paso
//            if (MapUtilities.PathToPointIsClear(position, point.position))
//            {
//                arePrioriryTarget = true;
//                var distance = point.position.Distance(position);
//                if (distance < bestTargetDistance)
//                {
//                    bestTargetDistance = distance;
//                    target = new PriorityGroupTarget() { TargetPosition = point.position, TargetEntity = pointEntity, TargetHex = point.position.Round() };
//                }
//            }
//        }
//    }
//    return arePrioriryTarget;
//}
//private static void FindAndAddPosibleTargetsToBuffer(FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, int[] teams, Entity[] entities, ColliderLayer[] colliderLayers, Fix64[] colliderRadiuses, Dictionary<Hex, List<SortPoint>> entitiesForeachHex, DynamicBuffer<BEPosibleTarget> buffer)
//{
//    Hex roundedPosition = position.Round();
//    var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

//    foreach (var point in pointsOfHex)
//    {
//        int pointTeam = teams[point.index];
//        Entity pointEntity = entities[point.index];
//        var pointLayer = colliderLayers[point.index];
//        var pointRadius = colliderRadiuses[point.index];


//        if (ValidPossibleTarget(position, sightRange, team.Number, point.position, pointLayer, filters, pointTeam))
//        {
//            buffer.Add(new BEPosibleTarget()
//            {
//                Position = point.position,
//                Radius = pointRadius,
//                Entity = pointEntity
//            });
//        }
//    }
//}
//private static bool ValidPossibleTarget(FractionalHex position, Fix64 sightRange, int team, FractionalHex pointPos, ColliderLayer pointLayer, ActTargetFilters filters, int pointTeam)
//{
//    if (pointPos.Distance(position) > sightRange || pointLayer == ColliderLayer.GROUP)
//    {
//        return false;
//    }
//    else
//    {
//        return PassTeamFilter(filters, team, pointTeam);
//    }
//}
//private static bool PassTeamFilter(ActTargetFilters filters, int team, int teamB)
//{
//    bool sameTeam = team == teamB;
//    if (sameTeam && filters.ActOnTeamates)
//    {
//        return true;
//    }
//    else if (!sameTeam && filters.ActOnEnemies)
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}
//}