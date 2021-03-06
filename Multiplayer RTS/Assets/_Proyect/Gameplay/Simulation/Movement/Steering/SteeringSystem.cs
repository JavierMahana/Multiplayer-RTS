﻿using FixMath.NET;
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
//I need to test the cohesion and alineation part
//I need to implemetn all the other parts like separation and avoidance
public class SteeringSystem : ComponentSystem
{
    private static readonly Fix64 WAIT_CHILD_WEIGHT = (Fix64)0.3;
    private const int SEPARATION_FOV = 20;

    private EntityQuery m_OnGroupUnitQuerry;
    private EntityQuery m_OnReinforcementUnitQuerry;
    private EntityQuery m_GroupEntityQuerry;

    protected override void OnCreate()
    {
        m_OnGroupUnitQuerry = World.EntityManager.CreateEntityQuery(
         typeof(OnGroup),
         typeof(Speed),
         typeof(Parent),
         typeof(DesiredMovement),
         typeof(Steering),
         typeof(SteeringTarget),
         typeof(HexPosition), 
         typeof(DirectionAverage));

        m_OnReinforcementUnitQuerry = World.EntityManager.CreateEntityQuery(
         typeof(OnReinforcement),
         typeof(Speed),
         typeof(DesiredMovement),
         typeof(Steering),
         typeof(SteeringTarget),
         typeof(HexPosition),
         typeof(DirectionAverage));
         

    }
    //el steering inicialmente será con aceleración infinita
    //como inicio lo unico que hará es mover
    //this implementation is based on the ECS samples one
    //collecting the data
    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;

        #region On Reinforcement Unit Steering

        Entities.WithAll<OnReinforcement>().ForEach((ref DesiredMovement desiredMovement, ref HexPosition position, ref Speed speed, ref SteeringTarget target) =>
        {
            var postionDelta = target.TargetPosition - position.HexCoordinates;
            var distance = postionDelta.Lenght();
            var maxSpeedMovementDistance = speed.Value * MainSimulationLoopSystem.SimulationDeltaTime;
            if (distance <= Fix64.Zero)
            {
                desiredMovement.Value = FractionalHex.Zero;
                return;
            }

            if (target.StopAtTarget)
            {
                if (distance <= maxSpeedMovementDistance)
                {
                    desiredMovement.Value = postionDelta.NormalizedManhathan() * distance;
                    return;
                }
            }
            desiredMovement.Value = postionDelta.NormalizedManhathan() * maxSpeedMovementDistance;
        });

        #endregion

        #region Unit On Group Steering
        int OnGroupUnitCount = m_OnGroupUnitQuerry.CalculateEntityCount();

        //voy a crear más basura de lo necesario por mas readibilidad.

        var entityToIndex = new Dictionary<int, int>(OnGroupUnitCount);
        var groupCohesions = new FractionalHex[OnGroupUnitCount];
        var groupAlignements = new FractionalHex[OnGroupUnitCount];
        var groupsUnitsHashMap = new Dictionary<int, List<int>>(OnGroupUnitCount);

        var groupCount = new int[OnGroupUnitCount];
        var groupIndices = new int[OnGroupUnitCount];

        var unitGroupalSeparations = new FractionalHex[OnGroupUnitCount];
        var unitSeparationValues   = new FractionalHex[OnGroupUnitCount];
        var unitPositions          = new FractionalHex[OnGroupUnitCount];
        var unitDirections         = new FractionalHex[OnGroupUnitCount];

        var unitSeparations               = new FractionalHex[OnGroupUnitCount];
        var unitSeparationCounts          = new int[OnGroupUnitCount];
        var unitSeparationDistances       = new Fix64[OnGroupUnitCount];
        var unitSingleSeparationDistances = new Fix64[OnGroupUnitCount];

        //made only because the foreach delegate don't have enough slots for all the components needed
        var unitSpeeds = new Fix64[OnGroupUnitCount];


        //InitialFillOnGroupSteeringCollections
        int entityCollectionIndex = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref Steering steering, ref Speed speed, ref HexPosition position, ref DirectionAverage directionAverage) =>
        {            
            groupCohesions[entityCollectionIndex]   = position.HexCoordinates;
            groupAlignements[entityCollectionIndex] = directionAverage.Value;
              
            unitSeparationCounts[entityCollectionIndex]          = 0;
            unitSeparationDistances[entityCollectionIndex]       = steering.separationDistance;
            unitSingleSeparationDistances[entityCollectionIndex] = steering.singleSeparationDistance;
            unitSpeeds[entityCollectionIndex]                    = speed.Value;
            unitPositions[entityCollectionIndex]                 = position.HexCoordinates;
            unitDirections[entityCollectionIndex]                = directionAverage.Value;

            //hash map
            int parentIndex = parent.ParentEntity.Index;
            List<int> sameGroupIndices;
            if (groupsUnitsHashMap.TryGetValue(parentIndex, out sameGroupIndices))
            {
                sameGroupIndices.Add(entityCollectionIndex);
                groupsUnitsHashMap[parentIndex] = sameGroupIndices;
            }
            else
            {
                sameGroupIndices = new List<int>();
                sameGroupIndices.Add(entityCollectionIndex);
                groupsUnitsHashMap.Add(parentIndex, sameGroupIndices);
            }


            entityToIndex.Add(entity.Index, entityCollectionIndex);


            entityCollectionIndex++;
        });


        CompleteOnGroupSteeringCollections(unitGroupalSeparations, unitSingleSeparationDistances, unitPositions, unitDirections, groupCohesions, groupAlignements, groupsUnitsHashMap, groupCount, groupIndices, unitSeparations, unitSeparationCounts, unitSeparationDistances);

        
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent,
            ref DesiredMovement desiredMovement, ref Steering steering, ref SteeringTarget target, ref HexPosition position, ref DirectionAverage directionAverage) =>
        {
            if (!entityToIndex.TryGetValue(entity.Index, out int entityIndex))
            {
                Debug.LogError("There aren't an index asociated with the current entity. Check the assignation of the entity to index dictionary");
            }


            var currentPosition = position.HexCoordinates;
            var targetPostion = target.TargetPosition;
            var satisfactionRadius = steering.satisfactionDistance;

            if (currentPosition.Distance(targetPostion) < satisfactionRadius) { desiredMovement.Value = FractionalHex.Zero; return; }

            var speed = unitSpeeds[entityIndex];
            var separationValue = unitSeparations[entityIndex];
            var groupalSeparationValues = unitGroupalSeparations[entityIndex];

            var separationCount = unitSeparationCounts[entityIndex];
            var groupIndex = groupIndices[entityIndex];
            var siblinCount = groupCount[groupIndex];
            var cohesionValue = groupCohesions[groupIndex];
            var alignementValue = groupAlignements[groupIndex];


            var cohesionWeight = steering.cohesionWeight;
            var alignementWeight = steering.alineationWeight;
            var separationWeight = steering.separationWeight;
            var groupalSeparationWeight = steering.groupalSeparationWeight;
            var flockWeight = steering.flockWeight;
            var targetWeight = steering.targetWeight;
            var previousDirectionWeight = steering.previousDirectionWeight;

            var groupalSeparationDirection = separationCount != 0
                                      ? (currentPosition - (groupalSeparationValues / separationCount)).NormalizedManhathan()
                                      : FractionalHex.Zero;
            var cohesionDirection = ((cohesionValue / siblinCount) - currentPosition).NormalizedManhathan();
            var alignementDirection = (alignementValue / siblinCount).NormalizedManhathan();

            var flockDirection = ((separationValue * separationWeight)
                                 + (cohesionDirection * cohesionWeight)
                                 + (alignementDirection * alignementWeight)
                                 + groupalSeparationDirection * groupalSeparationWeight)
                                 .Normalized();
            var targetDirection = (target.TargetPosition - currentPosition).NormalizedManhathan();
            var previousDirection = directionAverage.Value;
            var finalDirection = ((flockDirection * flockWeight)
                                 + (targetDirection * targetWeight)
                                 + (previousDirection * previousDirectionWeight))
                                 .NormalizedManhathan();
            
            var maxMovement = speed * MainSimulationLoopSystem.SimulationDeltaTime;

            desiredMovement.Value = finalDirection * maxMovement;

            if (NotFullSpeedMovementIsNeeded(
                finalDirection, maxMovement, currentPosition, target.TargetPosition, out FractionalHex closestPosiblePositionToTarget))
            {                      
                desiredMovement.Value = closestPosiblePositionToTarget - currentPosition;
            }
            else
            {                
                desiredMovement.Value = finalDirection * maxMovement;
            }
        });
        #endregion

        #region group steering
        var countOfChilds = new Dictionary<Entity, int>();
        var childPositionSum = new Dictionary<Entity, FractionalHex>();
        Entities.WithAll<OnGroup>().ForEach((Parent parent, ref HexPosition hexPosition) => 
        {
            var parentEntity = parent.ParentEntity;
            var pos = hexPosition.HexCoordinates;
            if (countOfChilds.ContainsKey(parentEntity))
            {
                countOfChilds[parentEntity] += 1;
                childPositionSum[parentEntity] += pos;
            }
            else 
            {
                countOfChilds.Add(parentEntity, 1);
                childPositionSum.Add(parentEntity, pos);
            }
        });


        Entities.WithAll<Group>().ForEach((Entity entity, ref DesiredMovement desiredMovement, ref HexPosition position, ref Speed speed, ref SteeringTarget target) =>
        {
            

            var postionDelta = target.TargetPosition - position.HexCoordinates;
            var distance = postionDelta.Lenght();

            
            if (!countOfChilds.ContainsKey(entity)) 
            {
                Debug.Log("there are a group without any child in the 'OnGroup' state the group will move to the closest open hex. _steering system_");
                if (map != null)
                {
                    var newTarget = (FractionalHex)MapUtilities.FindClosestOpenHex(position.HexCoordinates, map.map, true);
                    postionDelta = newTarget - position.HexCoordinates;
                    distance = postionDelta.Lenght();
                    var maxSpeedMovementDistance = speed.Value * MainSimulationLoopSystem.SimulationDeltaTime;
                    if (distance <= maxSpeedMovementDistance)
                    {
                        desiredMovement.Value = postionDelta.NormalizedManhathan() * distance;
                        return;
                    }
                    desiredMovement.Value = postionDelta.NormalizedManhathan() * maxSpeedMovementDistance;
                }
                else 
                {
                    Debug.Log("WTF why there is no map?");
                    desiredMovement.Value = FractionalHex.Zero;
                }
                
                
                
            }
            else
            {
                var childAveragePos = childPositionSum[entity] / math.max(1, countOfChilds[entity]);

                var desiredMovementDistance = GetMovementMagnitudeTowardsDesiredConsideringChildPos(
                    position.HexCoordinates,
                    childAveragePos,
                    target.TargetPosition,
                    speed.Value,
                    WAIT_CHILD_WEIGHT,
                    target.StopAtTarget);
                if (desiredMovementDistance <= (Fix64)Fix64.Precision)
                {
                    Debug.Log("desired movement distance = 0, check ther function <GetMovementMagnitudeTowardsDesiredConsideringChildPos>");
                }
                desiredMovementDistance *= MainSimulationLoopSystem.SimulationDeltaTime;
                var maxSpeedMovementDistance = speed.Value * MainSimulationLoopSystem.SimulationDeltaTime;


                if (distance <= Fix64.Zero)
                {
                    desiredMovement.Value = FractionalHex.Zero;
                    return;
                }

                if (target.StopAtTarget)
                {
                    if (distance <= desiredMovementDistance)
                    {
                        desiredMovement.Value = postionDelta.NormalizedManhathan() * distance;
                        return;
                    }
                }
                desiredMovement.Value = postionDelta.NormalizedManhathan() * desiredMovementDistance;
            }


            
        });

        #endregion
    }

    #region On Group collection filling methods

    private void CompleteOnGroupSteeringCollections(FractionalHex[]unitGroupalSeparations, Fix64[] unitSingleSeparationDistances, FractionalHex[] unitPositions, FractionalHex[] unitDirections, FractionalHex[] groupCohesion, FractionalHex[] groupAlinement, Dictionary<int, List<int>> groupsHashMap, int[] groupCount, int[] groupIndices, FractionalHex[] unitSeparations, int[] unitSeparationCounts, Fix64[] unitSeparationDistances)
    {
        foreach (var bucket in groupsHashMap)
        {
            var sameGroupIndices = bucket.Value;
            if (sameGroupIndices == null || sameGroupIndices.Count == 0) continue;

            //we use the cohesion before its changed
            //SetTheSeparationForEveryUnitInTheList(sameGroupIndices, unitSeparationDistances, unitSeparationCounts, unitSeparations, unitPositions);
            SetGroupalSeparationValue(sameGroupIndices, unitSeparationDistances, unitSeparationCounts, unitGroupalSeparations, unitPositions);
            SetTheSeparationValue(sameGroupIndices, unitPositions, unitDirections, unitSingleSeparationDistances, unitSeparations,true);

            SetTheCohesionAndAlienationGroupValues(groupCohesion, groupAlinement, groupCount, groupIndices, sameGroupIndices);
        }
    }
    private static void SetTheCohesionAndAlienationGroupValues
        (FractionalHex[] groupPositionsSum, FractionalHex[] groupDirectionsSum, int[] groupCount, int[] groupIndices, List<int> sameGroupIndices)
    {
        int firstIndex = sameGroupIndices[0];


        groupCount[firstIndex] = 1;
        groupIndices[firstIndex] = firstIndex;




        for (int i = 1; i < sameGroupIndices.Count; i++)
        {
            int index = sameGroupIndices[i];

            groupIndices[index] = firstIndex;

            groupCount[firstIndex] += 1;
            groupPositionsSum[firstIndex] = groupPositionsSum[firstIndex] + groupPositionsSum[index];
            groupDirectionsSum[firstIndex] = groupDirectionsSum[firstIndex] + groupDirectionsSum[index];
        }
    }
    /// <summary>
    /// it populates the "unitSeparationSum" array with the sum of the postions of the siblins that are close enoght, and it saves the count too.
    /// </summary>
    private void SetGroupalSeparationValue
    (List<int> indicesOfUnits, Fix64[] separationDistances, int[] unitSeparationCount, FractionalHex[] unitSeparationSum, FractionalHex[] unitPositions)
    {
        foreach (var unitIndex in indicesOfUnits)
        {
            FractionalHex unitPosition = unitPositions[unitIndex];
            Fix64 separationDistance = separationDistances[unitIndex];
            foreach (var index in indicesOfUnits)
            {
                if (index == unitIndex) continue;
                FractionalHex inspectingSiblinPosition = unitPositions[index];

                if (unitPosition.Distance(inspectingSiblinPosition) <= separationDistance)
                {
                    unitSeparationSum[unitIndex] += inspectingSiblinPosition;
                    unitSeparationCount[unitIndex] += 1;
                }
            }
        }

    }

    private void SetTheSeparationValue(List<int> indicesOfUnits, FractionalHex[] unitPositions, FractionalHex[] unitDirections, Fix64[]separationDistances, FractionalHex[] results, bool perpendicular = false)
    {
        foreach (var unitIndex in indicesOfUnits)
        {
            FractionalHex unitPosition = unitPositions[unitIndex];
            FractionalHex unitDirection = unitDirections[unitIndex].Normalized();
            Fix64 separationDistance = separationDistances[unitIndex];

            var siblinsInSD = new List<int>();
            foreach (var siblin in indicesOfUnits)
            {
                if (unitIndex == siblin) continue;
                var siblinPosition = unitPositions[siblin];
                var distance = siblinPosition.Distance(unitPosition);
                if (distance <= separationDistance) 
                {
                    siblinsInSD.Add(siblin);
                }
            }

            var siblinsInFOV = new List<int>();
            foreach (var siblinInSD in siblinsInSD)
            {
                var siblinPostion = unitPositions[siblinInSD];
                var angle = FractionalHex.Angle(unitPosition, unitDirection, siblinPostion);
                //siblinsInFOV.Add(siblinInSD);
                if (angle <= (Fix64)SEPARATION_FOV)
                {
                    siblinsInFOV.Add(siblinInSD);
                }
            }

            if (siblinsInFOV.Count == 0 || siblinsInFOV == null)
            {
                results[unitIndex] = FractionalHex.Zero;
                continue;
            }
            else
            {
                int closestSiblin = siblinsInFOV[0];
                var closestSiblinPos = unitPositions[closestSiblin];
                Fix64 closestSiblinDistance = unitPosition.Distance(unitPositions[closestSiblin]);
                for (int i = 1; i < siblinsInFOV.Count; i++)
                {
                    int siblin = siblinsInFOV[i];
                    var siblinPos = unitPositions[siblin];
                    Fix64 distance = unitPosition.Distance(siblinPos);
                    if (distance < closestSiblinDistance)
                    {
                        closestSiblin = siblin;
                        closestSiblinPos = siblinPos;
                        closestSiblinDistance = distance;
                    }
                }

                //here we set the direction of the separation
                if (perpendicular)
                {
                    var dirNormalized = unitDirection.Normalized();
                    var diference = closestSiblinPos - unitPosition;
                    var dot = FractionalHex.DotProduct(dirNormalized, diference);
                    var perpendicularPointInLine = dirNormalized * dot;


                    results[unitIndex] = (diference - perpendicularPointInLine).NormalizedManhathan(); 
                }
                else
                {
                    //contrary to the siblin
                    results[unitIndex] = (unitPosition - closestSiblinPos).NormalizedManhathan();
                }
                
                

            }

        }
    }
    #endregion


    /// <summary>
    /// se mueve siempre en la misma linea. lo que se hace es que se mueve a una velocidad la cual respeta la pocision de sus hijos
    /// </summary>  
    /// <param name="weight">is the ammount of weight the child positions have. 0 is none. 1 is max </param>
    private static Fix64 GetMovementMagnitudeTowardsDesiredConsideringChildPos(FractionalHex groupPos, FractionalHex childAveragePos, FractionalHex desiredPosition, Fix64 maxSpeed, Fix64 weight, bool stopAtDesired)
    {
        if (weight < Fix64.Zero || weight > Fix64.One) throw new System.ArgumentException("the weight parameter must be in the 0-1 range");

        var direction = (desiredPosition - groupPos).NormalizedManhathan();
        //usa valores euler para conseguir el punto con el que se saca el weight medio.
        var movementDirectionEuler = (desiredPosition - groupPos).Normalized();
        var childAverageRelativePos = (childAveragePos - groupPos);
        var distanceToChildEuler = FractionalHex.DotProduct(movementDirectionEuler, childAverageRelativePos);

        FractionalHex counterPos;
        Fix64 childPosAverageInfluence;
        if (distanceToChildEuler < Fix64.Zero)
        {
            counterPos = -movementDirectionEuler * distanceToChildEuler;
            childPosAverageInfluence = -(counterPos.Lenght());


            var lerpedInfluence = Fix64.Lerp(maxSpeed, Fix64.Min(childPosAverageInfluence, maxSpeed), weight);
            if (lerpedInfluence < Fix64.Zero)
            {
                lerpedInfluence = Fix64.Zero;
            }
            return lerpedInfluence;
        }
        else 
        {
            return maxSpeed;
        }


    }


    /// <returns>returns true if the closeest point is not the start niether the end of the segment</returns>
    public static bool NotFullSpeedMovementIsNeeded(FractionalHex direction, Fix64 maxSpeed, FractionalHex originPoint, FractionalHex targetPoint, out FractionalHex closestPointInMovementSegment)
    {
        var distanceFromTheClosestPointToTarget = FractionalHex.ClosestPointInLine(originPoint, direction, targetPoint);
        if (distanceFromTheClosestPointToTarget <= Fix64.Zero)
        {
            closestPointInMovementSegment = originPoint;
            return false;
        }
        else if (distanceFromTheClosestPointToTarget >= maxSpeed)
        {
            closestPointInMovementSegment = originPoint + (direction * maxSpeed);
            return false;
        }
        else 
        {
            closestPointInMovementSegment = originPoint + (direction * distanceFromTheClosestPointToTarget);
            return true;
        }
    }
}

