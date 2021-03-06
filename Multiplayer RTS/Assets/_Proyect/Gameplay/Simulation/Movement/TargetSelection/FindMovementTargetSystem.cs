﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using static Unity.Mathematics.math;
using FixMath.NET;
using UnityEngine;
using System;

[DisableAutoCreation]

// este sistema esta encargado de escribir en el steering target component.
//acá se diferencian entre distintos tipos de entidad, ya que en el sistema de steering todos son iguales.
 

//por ahora el sistema solo considera grupos de 9 como maximo para las formaciones
public class FindMovementTargetSystem : ComponentSystem
{
    private readonly static Fix64 MAX_DISTANCE_TO_GROUP_TO_USE_RELATIVE_POSITION = (Fix64) 0.7;
    private readonly static Fix64 TARGET_OFFSET = (Fix64)0.015;
    private readonly static Fix64 TARGET_OFFSET_FROM_GROUP_CENTER = (Fix64)0.3;
    private EntityQuery m_GroupQuery;
    private EntityQuery m_UnitOnGroupQuery;

    /// <summary>
    /// ordena comparando la diferencia de pocision entre las entidades y
    /// el promedio de todas las entidades del grupo con la pocision de la formacion
    /// </summary>
    private static int[] Sort(ICollection<int> unitsIndices, FractionalHex[] positions, FractionalHex[] formationSlots)
    {
        if(positions == null || formationSlots == null || unitsIndices == null)
        {
            throw new ArgumentException();
        }
        int length = unitsIndices.Count;

        var unsortedIndices = new List<int>(unitsIndices);
        int[] sortedIndices = new int[length];


        var positionAverage = FractionalHex.Zero;        
        foreach (int index in unitsIndices)
        {
            positionAverage += positions[index];
        }
        positionAverage /= length;



        for (int i = 0; i < length; i++)
        {
            var inspectingSlot = formationSlots[i];

            int bestUnitIndex = int.MaxValue;
            Fix64 bestUnitWeight = Fix64.MaxValue;
            foreach (int unitIndex in unsortedIndices)
            {                
                Fix64 weight = (positions[unitIndex] - positionAverage).Distance(inspectingSlot);
                if (weight < bestUnitWeight)
                {
                    bestUnitIndex = unitIndex;
                    bestUnitWeight = weight;
                }
            }

            Debug.Assert(bestUnitIndex != int.MaxValue, "the unsorted array run out of values");
            
                
            sortedIndices[i] = bestUnitIndex;
            unsortedIndices.Remove(bestUnitIndex);

            //int bestUnit = unsortedIndices[0];
            //Fix64 bestUnitWeight = (positions[bestUnit] - positionAverage).Distance(inspectingSlot);
            //for (int n = 1; n < unsortedIndices.Count; n++)
            //{
            //    int unit = unsortedIndices[n];
            //    Fix64 weight = (positions[unit] - positionAverage).Distance(inspectingSlot);
            //    if (weight < bestUnitWeight)
            //    {
            //        bestUnit = unit;
            //        bestUnitWeight = weight;
            //    }
            //}


        }

        return sortedIndices;
    }

    protected override void OnCreate()
    {
        m_GroupQuery = GetEntityQuery(typeof(Group), typeof(HexPosition), typeof(DirectionAverage));
        m_UnitOnGroupQuery = GetEntityQuery(typeof(OnGroup), typeof(HexPosition), typeof(SteeringTarget), typeof(ActionAttributes));
    }
    protected override void OnUpdate()
    {


        #region On group movement target find.

        #region init collections
        int groupCount = m_GroupQuery.CalculateEntityCount();
        var groupPosition = new FractionalHex[groupCount];
        var groupDirections = new FractionalHex[groupCount];
        var groupReachedDestination = new bool[groupCount];         
        var groupIndices = new Dictionary<int, int>(groupCount);
        var groupTriggers = new bool[groupCount];

        int onGroupCount         = m_UnitOnGroupQuery.CalculateEntityCount();
        var onGroupEntityToIndex = new Dictionary<int, int>(onGroupCount);
        var onGroupPositions     = new FractionalHex[onGroupCount];
        var onGroupActRange      = new Fix64[onGroupCount];
        var onGroupIsMelee       = new bool[onGroupCount];
        var onGroupRadius        = new Fix64[onGroupCount];
        var groupMemberHashMap   = new Dictionary<int, List<int>>(onGroupCount);
        var unitsParents         = new Entity[onGroupCount];

        var unitsTempTargets = new FractionalHex[onGroupCount];



        //initializing group collections
        int groupIterator = 0;
        Entities.WithAll<Group>().ForEach((Entity entity, ref HexPosition position, ref DirectionAverage direction, ref MovementState movementState) =>
        {
            groupPosition[groupIterator] = position.HexCoordinates;
            groupReachedDestination[groupIterator] = movementState.DestinationReached;
            groupDirections[groupIterator] = direction.Value;
            groupIndices.Add(entity.Index, groupIterator);
            groupIterator++;
        });

        //initializing unit collections
        int startupCollectionIndex = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity) =>
        {
            onGroupEntityToIndex.Add(entity.Index, startupCollectionIndex);
            startupCollectionIndex++;
        });
        Entities.WithAll<OnGroup>().ForEach((Entity entity, ref HexPosition position, ref Collider collider, ref ActionAttributes actAttributes) =>
        {
            int collectionIndex = onGroupEntityToIndex[entity.Index];
            onGroupPositions[collectionIndex] = position.HexCoordinates;
            onGroupRadius[collectionIndex] = collider.Radius;
            onGroupActRange[collectionIndex] = actAttributes.ActRange;
            onGroupIsMelee[collectionIndex] = actAttributes.Melee;
        });
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent) =>
        {
            int collectionIndex = onGroupEntityToIndex[entity.Index];
            int parentIndex = parent.ParentEntity.Index;
            unitsParents[collectionIndex] = parent.ParentEntity;

            List<int> sameGroupIndices;
            if (groupMemberHashMap.TryGetValue(parentIndex, out sameGroupIndices))
            {
                sameGroupIndices.Add(collectionIndex);
                groupMemberHashMap[parentIndex] = sameGroupIndices;
            }
            else
            {
                sameGroupIndices = new List<int>();
                sameGroupIndices.Add(collectionIndex);
                groupMemberHashMap.Add(parentIndex, sameGroupIndices);
            }
        });
        #endregion

        #region writes default steering targets(NO ACTION TARGET)
        //goes for each group and sets the temp targets
        //the temp target are;
        //when moving: the position of the group offsetted by the direction of the group and the position of the unit relative to its siblins.
        //when standing: the formation position.
        foreach (var groupUnitList in groupMemberHashMap)
        {
            var siblinList = new List<int>(groupUnitList.Value);
            int siblinCount = siblinList.Count;
            if (!FormationSlots.Slots.TryGetValue(new int2(siblinCount, 9), out var formationSlots))
            { Debug.LogError($"There isn't a formation for the ammount of siblins that this group have: {siblinCount}|9"); continue; }

            var parentEntity             = unitsParents[siblinList[0]];
            int parentEntityIndex        = parentEntity.Index;
            int parentIndex              = groupIndices[parentEntityIndex];
            var parentPosition           = groupPosition[parentIndex];
            var parentReachedDestination = groupReachedDestination[parentIndex];
            var parentDirection          = groupDirections[parentIndex];

            bool parentHaveStoped = parentDirection == FractionalHex.Zero; 

            if (parentReachedDestination || parentHaveStoped)
            {
                int[] sortedSiblins = Sort(siblinList, onGroupPositions, formationSlots);
                for (int i = 0; i < sortedSiblins.Length; i++)
                {
                    int currUnitIndex = sortedSiblins[i];
                    unitsTempTargets[currUnitIndex] = parentPosition + formationSlots[i];
                }
            }
            else
            {
                //moving behaviour
                foreach (var currUnitIndex in siblinList)
                {
                    var pos = onGroupPositions[currUnitIndex];
                    var posDiference = pos - parentPosition;

                    if (pos.Distance(parentPosition) > MAX_DISTANCE_TO_GROUP_TO_USE_RELATIVE_POSITION)
                    {
                        unitsTempTargets[currUnitIndex] = (parentPosition + (parentDirection * TARGET_OFFSET_FROM_GROUP_CENTER));
                    }
                    else
                    {
                        unitsTempTargets[currUnitIndex] = (parentPosition + (parentDirection * TARGET_OFFSET_FROM_GROUP_CENTER)) + posDiference;
                    }
                }
            }
        }
        //writes on the steering target component
        //sin target de accion -> es decir sigue el comportamiento por defecto de los grupos.
        //aka: utiliza el tempTargets asignado anteriormente
        Entities.WithAll<OnGroup, PathWaypoint>().WithNone<ActionTarget>().ForEach((Entity entity, Parent parent, ref HexPosition hexPos, ref PathWaypointIndex waypointIndex, ref SteeringTarget target) =>
        {
            var pos = hexPos.HexCoordinates;
            int parentGroupIndex;
            int collectionIndex = onGroupEntityToIndex[entity.Index];

            if (!groupIndices.TryGetValue(parent.ParentEntity.Index, out parentGroupIndex))
            {
                Debug.LogError($"This entity: {parent.ParentEntity} is a parent and it needs to have a Group, hexPosition, and a directionAverage component");
                return;
            }
            bool parentReachedDestination = groupReachedDestination[parentGroupIndex];


            var desiredTarget = unitsTempTargets[collectionIndex];
            if (MapUtilities.PathToPointIsClear(pos, desiredTarget))
            {
                target.TargetPosition = desiredTarget;
                if (parentReachedDestination)
                    target.StopAtTarget = true;
                else
                    target.StopAtTarget = false;
            }
            else 
            {
                var buffer = EntityManager.GetBuffer<PathWaypoint>(entity);

                if (waypointIndex.Value >= buffer.Length)
                {
                    PostUpdateCommands.AddComponent<RefreshPathNow>(entity);
                    target.TargetPosition = pos;
                    return;
                }


                if (waypointIndex.Value == buffer.Length - 1)
                    target.StopAtTarget = true;
                else
                    target.StopAtTarget = false;


                target.TargetPosition = (FractionalHex)buffer[waypointIndex.Value].Value;

            }

            //target.TargetPosition = unitsTempTargets[collectionIndex];


            //if (parentReachedDestination)
            //    target.StopAtTarget = true;
            //else
            //    target.StopAtTarget = false;
        });
        #endregion

        #region writes steering targets WITH ACTION TARGET
        //with action target
        Entities.WithAll<OnGroup, PathWaypoint>().ForEach((Entity entity, Parent parent, ref ActionTarget actionTarget, ref SteeringTarget steeringTarget, ref PathWaypointIndex waypointIndex, ref Steering steeringValues) =>
        {
            int onGroupIndex = onGroupEntityToIndex[entity.Index];
            var pos          = onGroupPositions[onGroupIndex];
            var radius       = onGroupRadius[onGroupIndex];
            var actRange     = onGroupActRange[onGroupIndex];
            bool isMelee     = onGroupIsMelee[onGroupIndex];
            var distance     = pos.Distance(actionTarget.TargetPosition);
            

            bool isActing = EntityManager.HasComponent<IsActing>(entity);

            if (!isActing)
            {
                var directionToTarget = (actionTarget.TargetPosition - pos).NormalizedManhathan();
                var steeringTargetPos = pos + (directionToTarget * (distance - (actionTarget.TargetRadius + radius + actRange)));



                bool pathToTargetIsClear;
                if (isMelee)
                {
                    if (actionTarget.OccupiesFullHex)
                    {
                        pathToTargetIsClear = MapUtilities.PathToPointIsClear(pos, (FractionalHex)actionTarget.OccupyingHex, pathIsClearEvenIfDestPointIsBlocked: true);
                    }
                    else
                    {
                        pathToTargetIsClear = MapUtilities.PathToPointIsClear(pos, steeringTargetPos);
                    }
                }
                else
                {
                    pathToTargetIsClear = MapUtilities.PathToPointIsClear(pos, steeringTargetPos);
                }
                

                if (pathToTargetIsClear)
                {
                    steeringTarget.TargetPosition = pos + (directionToTarget * (distance - (actionTarget.TargetRadius + radius + actRange - steeringValues.satisfactionDistance)));
                    steeringTarget.StopAtTarget = true;
                }
                else
                {
                    var buffer = EntityManager.GetBuffer<PathWaypoint>(entity);

                    if (waypointIndex.Value >= buffer.Length)
                    {
                        PostUpdateCommands.AddComponent<RefreshPathNow>(entity);
                        steeringTarget.TargetPosition = pos;
                        return;
                    }

                    
                    if (waypointIndex.Value == buffer.Length - 1)
                        steeringTarget.StopAtTarget = true;
                    else
                        steeringTarget.StopAtTarget = false;
                    

                    steeringTarget.TargetPosition = (FractionalHex)buffer[waypointIndex.Value].Value;

                }
            }
            else //los que estan enmedio de una accion de quedan quietos.
            {
                steeringTarget.TargetPosition = pos;
                steeringTarget.StopAtTarget = true;
            }
        });
        #endregion


        #endregion


        #region On Reinforcement and on group target Set
        Entities.WithAll<PathWaypoint>().WithAny<OnReinforcement, Group>().ForEach(
        (Entity entity, ref HexPosition position, ref SteeringTarget target, ref PathWaypointIndex waypointIndex) =>
        {
            var buffer = EntityManager.GetBuffer<PathWaypoint>(entity);

            if (waypointIndex.Value >= buffer.Length)
            {
                //Debug.Log("There are not waypoints for the current index");
                target.TargetPosition = position.HexCoordinates;
                return;
            }

            if (waypointIndex.Value == buffer.Length - 1)
                 target.StopAtTarget = true;
            else
                target.StopAtTarget = false;

            target.TargetPosition = (FractionalHex)buffer[waypointIndex.Value].Value;
        });
        #endregion
    }

}

//private static bool StopAtTarget
//private readonly static Fix64 SORTING_HEIGHT_DIFERENCE_TOLERANCE = (Fix64)0.12;
//no es nada optimo este algoritmo y generaba un comportamineto no deseado
//private static int[] Sort(ICollection<int> unitsIndices, FractionalHex[] positions, bool doubleCheck = true)
//{
//    int length = unitsIndices.Count;
//    var unsortedIndices = new List<int>(unitsIndices);
//    int[] sortedIndices = new int[length];

//    int sortedSlot = 0;

//    if (doubleCheck)
//    {
//        while (unsortedIndices.Count > 0)
//        {
//            Fix64 lowerUnitHeight = positions[unsortedIndices[0]].r;
//            for (int i = 1; i < unsortedIndices.Count; i++)
//            {
//                int index = unsortedIndices[i];
//                Fix64 height = positions[index].r;

//                if (height < lowerUnitHeight)
//                {
//                    lowerUnitHeight = height;
//                }
//            }
//            var similarHeightUnits = new List<int>();
//            foreach (int index in unsortedIndices)
//            {
//                Fix64 height = positions[index].r;

//                if (height <= lowerUnitHeight + SORTING_HEIGHT_DIFERENCE_TOLERANCE)
//                {
//                    similarHeightUnits.Add(index);
//                }
//            }
//            Debug.Assert(similarHeightUnits != null && similarHeightUnits.Count > 0, "We run into a problem searching the similar height units");

//            while (similarHeightUnits.Count > 0)
//            {
//                int leftmostUnitIndex = similarHeightUnits[0];
//                Fix64 leftmostUnitPositition = positions[leftmostUnitIndex].q;

//                for (int i = 1; i < similarHeightUnits.Count; i++)
//                {
//                    int index = similarHeightUnits[i];
//                    Fix64 horizontalPos = positions[index].r;

//                    if (horizontalPos < leftmostUnitPositition)
//                    {
//                        leftmostUnitIndex = index;
//                        leftmostUnitPositition = horizontalPos;
//                    }
//                }

//                sortedIndices[sortedSlot] = leftmostUnitIndex;
//                similarHeightUnits.Remove(leftmostUnitIndex);
//                unsortedIndices.Remove(leftmostUnitIndex);

//                if (sortedSlot > length) { Debug.LogError("infinite loop on the iner 'while' of the sorting"); break; }
//                sortedSlot++;
//            }
//            if (sortedSlot > length) { Debug.LogError("infinite loop on the outer 'while' of the sorting"); break; }
//        }
//        return sortedIndices;

//    }

//    //single check
//    else
//    {
//        while (unsortedIndices.Count > 0)
//        {
//            int lowerUnitIndex = unsortedIndices[0];
//            Fix64 lowerUnitHeight = positions[lowerUnitIndex].r;

//            for (int i = 1; i < unsortedIndices.Count; i++)
//            {
//                int index = unsortedIndices[i];
//                Fix64 height = positions[index].r;

//                if (height < lowerUnitHeight)
//                {
//                    lowerUnitIndex = index;
//                    lowerUnitHeight = height;
//                }
//            }

//            sortedIndices[sortedSlot] = lowerUnitIndex;
//            unsortedIndices.Remove(lowerUnitIndex);
//            sortedSlot++;
//        }
//        return sortedIndices;
//    }

//}
