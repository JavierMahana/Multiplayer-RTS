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
        List<EntityOnVision> unitsOnVisionTeam_0 = SightSystem.GetUnitsOnVisionOfTeam(0);
        List<EntityOnVision> unitsOnVisionTeam_1 = SightSystem.GetUnitsOnVisionOfTeam(1);
        List<EntityOnVision> unitsOnVisionTeam_2 = SightSystem.GetUnitsOnVisionOfTeam(2);
        List<EntityOnVision> unitsOnVisionTeam_3 = SightSystem.GetUnitsOnVisionOfTeam(3);
        List<EntityOnVision> unitsOnVisionTeam_4 = SightSystem.GetUnitsOnVisionOfTeam(4);
        List<EntityOnVision> unitsOnVisionTeam_5 = SightSystem.GetUnitsOnVisionOfTeam(5);
        List<EntityOnVision> unitsOnVisionTeam_6 = SightSystem.GetUnitsOnVisionOfTeam(6);
        List<EntityOnVision> unitsOnVisionTeam_7 = SightSystem.GetUnitsOnVisionOfTeam(7);


        var sortedByHex_UnitsOnVisionTeam_0 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_0);
        var sortedByHex_UnitsOnVisionTeam_1 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_1);
        var sortedByHex_UnitsOnVisionTeam_2 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_2);
        var sortedByHex_UnitsOnVisionTeam_3 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_3);
        var sortedByHex_UnitsOnVisionTeam_4 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_4);
        var sortedByHex_UnitsOnVisionTeam_5 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_5);
        var sortedByHex_UnitsOnVisionTeam_6 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_6);
        var sortedByHex_UnitsOnVisionTeam_7 = SpartialSortUtils.GetHexToPointsDictionary(unitsOnVisionTeam_7);



        var buildingsOnVisionTeam_0 = SightSystem.GetBuildingsOnVisionOfTeam(0);
        var buildingsOnVisionTeam_1 = SightSystem.GetBuildingsOnVisionOfTeam(1);
        var buildingsOnVisionTeam_2 = SightSystem.GetBuildingsOnVisionOfTeam(2);
        var buildingsOnVisionTeam_3 = SightSystem.GetBuildingsOnVisionOfTeam(3);
        var buildingsOnVisionTeam_4 = SightSystem.GetBuildingsOnVisionOfTeam(4);
        var buildingsOnVisionTeam_5 = SightSystem.GetBuildingsOnVisionOfTeam(5);
        var buildingsOnVisionTeam_6 = SightSystem.GetBuildingsOnVisionOfTeam(6);
        var buildingsOnVisionTeam_7 = SightSystem.GetBuildingsOnVisionOfTeam(7);



        //normal group entities.
        #region Group that targets entities on vition.

        Entities.WithAll<Group, BEPosibleTarget>().WithNone<GroupOnGather>().ForEach((Entity entity, ref HexPosition hexPosition, ref SightRange sightRange, ref GroupBehaviour behaviour, ref ActTargetFilters filters, ref Team team, ref MovementState movementState) =>
        {
            var buffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            buffer.Clear();

            var sortedByHex_UnitsOnTeamVision = GetSortedUnitsOnTeamVision(team.Number, 
                    sortedByHex_UnitsOnVisionTeam_0, sortedByHex_UnitsOnVisionTeam_1, sortedByHex_UnitsOnVisionTeam_2, sortedByHex_UnitsOnVisionTeam_3,
                    sortedByHex_UnitsOnVisionTeam_4, sortedByHex_UnitsOnVisionTeam_5, sortedByHex_UnitsOnVisionTeam_6, sortedByHex_UnitsOnVisionTeam_7);
            var buildingsOnTeamVision = GetBuildingOnTeamVision(team.Number,
                    buildingsOnVisionTeam_0, buildingsOnVisionTeam_1, buildingsOnVisionTeam_2, buildingsOnVisionTeam_3,
                    buildingsOnVisionTeam_4, buildingsOnVisionTeam_5, buildingsOnVisionTeam_6, buildingsOnVisionTeam_7);

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
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, sortedByHex_UnitsOnTeamVision, buildingsOnTeamVision, buffer);
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
                    FindAndAddPosibleTargetsToBuffer(hexPosition.HexCoordinates, sightRange.Value, filters, team, sortedByHex_UnitsOnTeamVision, buildingsOnTeamVision, buffer);
                    break;


                    
                case Behaviour.AGRESIVE://este modo agrega la entidad más cercana no bloqueada, como blanco prioritario. agrega a todas las entidades cercanas como posibles targets.
                    
                    PriorityGroupTarget priorityTarget;
                    if (GetPriorityTargetAndAddPosibleTargets(out priorityTarget, hexPosition.HexCoordinates, sightRange.Value, team, filters, sortedByHex_UnitsOnTeamVision, buildingsOnTeamVision, buffer))
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
        #endregion

        #region Groups that are gathering
        
        Entities.WithAll<Group, BEPosibleTarget, BEResourceSource>().ForEach((Entity entity, ref GroupOnGather onGather, ref HexPosition hexPosition, ref SightRange sightRange, ref Team team, ref MovementState movementState) => 
        {
            //Los targets son: todos los elementos del BEResourceSource + todos los elementos de los droppoints
            var posTargetBuffer = EntityManager.GetBuffer<BEPosibleTarget>(entity);

            posTargetBuffer.Clear();

            var resSourceBuffer = EntityManager.GetBuffer<BEResourceSource>(entity);
            var allDropPointsOfTeam = DropPointSystem.GetAllDropPointsOfTeam(onGather.GatheringResourceType, team.Number);

            foreach (var resSource in resSourceBuffer)
            {
                if (resSource.currentGatherers >= resSource.maxGatherers)
                {
                    continue;
                }

                var posTarget = new BEPosibleTarget()
                {
                    Entity = resSource.entity,
                    Position = (FractionalHex)resSource.position,
                    Radius = Fix64.Zero,
                    IsUnit = false,
                    GatherTarget = true,
                    IsResource = true,
                    OccupiesFullHex = true
                };
                posTargetBuffer.Add(posTarget);
            }


            foreach (var dropPoint in allDropPointsOfTeam)
            {
                var posTarget = new BEPosibleTarget()
                {
                    Entity = dropPoint.Value,
                    Position = (FractionalHex)dropPoint.Key,
                    Radius = Fix64.Zero,
                    IsUnit = false,
                    GatherTarget = true,
                    IsResource = true,
                    OccupiesFullHex = true
                };
                posTargetBuffer.Add(posTarget);
            }
        });

        #endregion
    }

    private static Dictionary<Hex, List<EntityOnVision>> GetSortedUnitsOnTeamVision(int team, Dictionary<Hex, List<EntityOnVision>> UOVTeam_0,
        Dictionary<Hex, List<EntityOnVision>> UOVTeam_1, Dictionary<Hex, List<EntityOnVision>> UOVTeam_2, Dictionary<Hex, List<EntityOnVision>> UOVTeam_3,
        Dictionary<Hex, List<EntityOnVision>> UOVTeam_4, Dictionary<Hex, List<EntityOnVision>> UOVTeam_5, Dictionary<Hex, List<EntityOnVision>> UOVTeam_6,
        Dictionary<Hex, List<EntityOnVision>> UOVTeam_7)
    {
        switch (team)
        {
            case 0:
                return UOVTeam_0;
            case 1:
                return UOVTeam_1;
            case 2:
                return UOVTeam_2;
            case 3:
                return UOVTeam_3;
            case 4:
                return UOVTeam_4;
            case 5:
                return UOVTeam_5;
            case 6:
                return UOVTeam_6;
            case 7:
                return UOVTeam_7;
            default:
                throw new System.ArgumentException("The team must be in the 0-7 range. other options are not implementedS");

        }
    }
    private static List<BuildingOnVision> GetBuildingOnTeamVision(int team, List<BuildingOnVision> BOVTeam_0, List<BuildingOnVision> BOVTeam_1,
        List<BuildingOnVision> BOVTeam_2, List<BuildingOnVision> BOVTeam_3, List<BuildingOnVision> BOVTeam_4,
        List<BuildingOnVision> BOVTeam_5, List<BuildingOnVision> BOVTeam_6, List<BuildingOnVision> BOVTeam_7)
    {
        switch (team)
        {
            case 0:
                return BOVTeam_0;
            case 1:
                return BOVTeam_1;
            case 2:
                return BOVTeam_2;
            case 3:
                return BOVTeam_3;
            case 4:
                return BOVTeam_4;
            case 5:
                return BOVTeam_5;
            case 6:
                return BOVTeam_6;
            case 7:
                return BOVTeam_7;
            default:
                throw new System.ArgumentException("The team must be in the 0-7 range. other options are not implementedS");

        }
    }
    /// <summary>
    /// Gets the target if true.
    /// debo ver una buena manera de conseguir el parent entity.
    /// </summary>
    private static bool GetPriorityTargetAndAddPosibleTargets(out PriorityGroupTarget target, FractionalHex position, Fix64 sightRange, Team team, ActTargetFilters filters, Dictionary<Hex, List<EntityOnVision>> entitiesForeachHex, List<BuildingOnVision> buildingsOnSight, DynamicBuffer<BEPosibleTarget> buffer)
    {
        Hex roundedPosition = position.Round();
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        target = new PriorityGroupTarget();
        var bestTargetDistance = Fix64.MaxValue;
        bool arePrioriryTarget = false;
        bool areUnitPriorityTarget = false;

        //units firs because we search if there is a unit in sight and if so there cannot be a building as a priority target.
        foreach (var point in pointsOfHex)
        {
            int pointTeam = point.team;
            Entity pointEntity = point.entity;
            var pointLayer = point.collider.Layer;
            var pointRadius = point.collider.Radius;


            if (ValidPossibleTarget(position, sightRange, team.Number, filters, point.position, pointRadius, pointTeam))
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = point.position,
                    Radius = pointRadius,
                    Entity = pointEntity,
                    IsUnit = true,
                    GatherTarget = false,
                    OccupiesFullHex = false
                });
                //hay que ver si hay algo que bloquea el paso
                if (MapUtilities.PathToPointIsClear(position, point.position))
                {
                    areUnitPriorityTarget = true;
                    arePrioriryTarget = true;
                    var distance = point.position.Distance(position);
                    if (distance < bestTargetDistance)
                    {
                        bestTargetDistance = distance;
                        target = new PriorityGroupTarget() {
                            //TargetPosition = point.position,
                            //TargetEntity = pointEntity,
                            TargetHex = point.position.Round(),
                            //IsUnit = true
                        };
                    }
                }
            }
        }

        //buildings
        foreach (var building in buildingsOnSight)
        {
            var buildingPos = (FractionalHex)building.building.position;

            if (ValidPossibleTarget(position, sightRange, team.Number, filters, buildingPos, (Fix64)0.5, building.team))
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = buildingPos,
                    Radius = Fix64.Zero,
                    Entity = building.entity,
                    IsUnit = false,
                    GatherTarget = false,
                    OccupiesFullHex = true                    
                });
            }
            if(!areUnitPriorityTarget) 
            {
                if (MapUtilities.PathToPointIsClear(position, buildingPos))
                {
                    arePrioriryTarget = true;
                    var distance = buildingPos.Distance(position);
                    if (distance < bestTargetDistance)
                    {
                        bestTargetDistance = distance;
                        target = new PriorityGroupTarget() {
                            //TargetPosition = buildingPos, 
                            //TargetEntity = building.entity,
                            TargetHex = buildingPos.Round(),
                            //IsUnit = false
                        };
                    }
                }
            }            
        }

        return arePrioriryTarget;
    }
    private static void FindAndAddPosibleTargetsToBuffer(FractionalHex position, Fix64 sightRange, ActTargetFilters filters, Team team, Dictionary<Hex, List<EntityOnVision>> entitiesForeachHex, List<BuildingOnVision> buildingsOnSight, DynamicBuffer<BEPosibleTarget> buffer)
    {        
        Hex roundedPosition = position.Round();

        //buildings
        foreach (var building in buildingsOnSight)
        {
            var buildingPos = (FractionalHex)building.building.position;
            
            if(ValidPossibleTarget(position, sightRange, team.Number, filters, buildingPos, (Fix64)0.5, building.team)) 
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = buildingPos,
                    Radius = Fix64.Zero,
                    Entity = building.entity,
                    IsUnit = false,
                    OccupiesFullHex = true,
                    GatherTarget = false,
                });
            }
        }


        //units
        var pointsOfHex = SpartialSortUtils.GetAllPointsAtRange(roundedPosition, (int)Fix64.Ceiling(sightRange), entitiesForeachHex);

        foreach (var point in pointsOfHex)
        {
            int pointTeam = point.team;
            Entity pointEntity = point.entity;
            var pointRadius = point.collider.Radius;

            
            if (ValidPossibleTarget(position, sightRange, team.Number, filters, point.position, pointRadius , pointTeam))
            {
                buffer.Add(new BEPosibleTarget()
                {
                    Position = point.position,
                    Radius = pointRadius,
                    Entity = pointEntity,
                    IsUnit = true,
                    OccupiesFullHex = false,
                    GatherTarget = false,
                }); 
            }
        }
    }
    private static bool ValidPossibleTarget(FractionalHex position, Fix64 sightRange, int team, ActTargetFilters filters, FractionalHex pointPos, Fix64 pointRadius, int pointTeam)
    {
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