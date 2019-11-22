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
//I need to test the cohesion and alineation part
//I need to implemetn all the other parts like separation and avoidance
public class SteeringSystem : ComponentSystem
{
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
        #region On Reinforcement Unit Steering and group steering

        Entities.WithAny<OnReinforcement, Group>().ForEach((ref DesiredMovement desiredMovement, ref HexPosition position, ref Speed speed, ref SteeringTarget target) =>
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
                    desiredMovement.Value = postionDelta.Normalized() * distance;
                    return;
                }
            }
            desiredMovement.Value = postionDelta.Normalized() * maxSpeedMovementDistance;
        });

        #endregion

        #region Unit On Group Steering
        int OnGroupUnitCount = m_OnGroupUnitQuerry.CalculateEntityCount();



        var entityToIndex = new Dictionary<int, int>(OnGroupUnitCount);
        var groupCohesions = new FractionalHex[OnGroupUnitCount];
        var groupAlignements = new FractionalHex[OnGroupUnitCount];
        var groupsHashMap = new Dictionary<int, List<int>>(OnGroupUnitCount);

        var groupCount = new int[OnGroupUnitCount];
        var groupIndices = new int[OnGroupUnitCount];

        var unitSeparations = new FractionalHex[OnGroupUnitCount];
        var unitSeparationCounts = new int[OnGroupUnitCount];
        var unitSeparationDistances = new Fix64[OnGroupUnitCount];

        //made only because the foreach delegate don't have enough slots for all the components needed
        var unitSpeeds = new Fix64[OnGroupUnitCount];


        //InitialFillOnGroupSteeringCollections
        int entityCollectionIndex = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref Steering steering, ref Speed speed, ref HexPosition position, ref DirectionAverage directionAverage) =>
        {            
            groupCohesions[entityCollectionIndex] = position.HexCoordinates;
            groupAlignements[entityCollectionIndex] = directionAverage.Value;
            //groupCount[entityCollectionIndex] = 1;
            unitSeparationCounts[entityCollectionIndex] = 0;
            unitSeparationDistances[entityCollectionIndex] = steering.separationDistance;
            unitSpeeds[entityCollectionIndex] = speed.Value;

            //hash map
            int parentIndex = parent.ParentEntity.Index;
            List<int> sameGroupIndices;
            if (groupsHashMap.TryGetValue(parentIndex, out sameGroupIndices))
            {
                sameGroupIndices.Add(entityCollectionIndex);
                groupsHashMap[parentIndex] = sameGroupIndices;
            }
            else
            {
                sameGroupIndices = new List<int>();
                sameGroupIndices.Add(entityCollectionIndex);
                groupsHashMap.Add(parentIndex, sameGroupIndices);
            }


            entityToIndex.Add(entity.Index, entityCollectionIndex);


            entityCollectionIndex++;
        });


        CompleteOnGroupSteeringCollections(groupCohesions, groupAlignements, groupsHashMap, groupCount, groupIndices, unitSeparations, unitSeparationCounts, unitSeparationDistances);

        
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
            var separationCount = unitSeparationCounts[entityIndex];
            var groupIndex = groupIndices[entityIndex];
            var siblinCount = groupCount[groupIndex];
            var cohesionValue = groupCohesions[groupIndex];
            var alignementValue = groupAlignements[groupIndex];

            var cohesionWeight = steering.cohesionWeight;
            var alignementWeight = steering.alineationWeight;
            var separationWeight = steering.separationWeight;
            var flockWeight = steering.flockWeight;
            var targetWeight = steering.targetWeight;
            var previousDirectionWeight = steering.previousDirectionWeight;

            var separationDirection = separationCount != 0
                                      ? (currentPosition - (separationValue / separationCount)).Normalized()
                                      : FractionalHex.Zero;
            var cohesionDirection = ((cohesionValue / siblinCount) - currentPosition).Normalized();
            var alignementDirection = (alignementValue / siblinCount).Normalized();

            var flockDirection = ((separationDirection * separationWeight)
                                 + (cohesionDirection * cohesionWeight)
                                 + (alignementDirection * alignementWeight))
                                 .Normalized();
            var targetDirection = (target.TargetPosition - currentPosition).Normalized();
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
    }

    #region On Group collection filling methods
    private void InitialFillOnGroupSteeringCollections(
        Dictionary<int, int> entityToIndex, FractionalHex[] groupCohesions, FractionalHex[] groupAlingnements, Fix64[] unitSpeeds,
        Dictionary<int, List<int>> groupHashMap,
        int[] unitSeparationCount, Fix64[] unitSeparationDistance)
    {
        int entityIndex = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref Steering steering, ref Speed speed, ref HexPosition position, ref DirectionAverage directionAverage) =>
        {

            groupCohesions[entityIndex] = position.HexCoordinates;
            groupAlingnements[entityIndex] = directionAverage.Value;
            unitSeparationCount[entityIndex] = 0;
            unitSeparationDistance[entityIndex] = steering.separationDistance;
            unitSpeeds[entityIndex] = speed.Value;

            
            int parentIndex = parent.ParentEntity.Index;
            List<int> sameGroupIndices;
            if (groupHashMap.TryGetValue(parentIndex, out sameGroupIndices))
            {
                sameGroupIndices.Add(entityIndex);
                groupHashMap[parentIndex] = sameGroupIndices;
            }
            else
            {
                sameGroupIndices = new List<int>();
                sameGroupIndices.Add(entityIndex);
                groupHashMap.Add(parentIndex, sameGroupIndices);
            }
            

            entityToIndex.Add(entity.Index, entityIndex);


            entityIndex++;
        });
    }
    private void CompleteOnGroupSteeringCollections(FractionalHex[] groupCohesion, FractionalHex[] groupAlinement, Dictionary<int, List<int>> groupsHashMap, int[] groupCount, int[] groupIndices, FractionalHex[] unitSeparations, int[] unitSeparationCounts, Fix64[] unitSeparationDistances)
    {
        foreach (var bucket in groupsHashMap)
        {
            var sameGroupIndices = bucket.Value;
            if (sameGroupIndices == null || sameGroupIndices.Count == 0) continue;

            //we use the cohesion before its changed
            SetTheSeparationForEveryUnitInTheList(sameGroupIndices, unitSeparationDistances, unitSeparationCounts, unitSeparations, groupCohesion);

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
    private void SetTheSeparationForEveryUnitInTheList
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
    #endregion


    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction">IMPORTANT: this must be a normalized vector</param>
    /// <param name="maxSpeed"></param>
    /// <param name="originPoint"></param>
    /// <param name="targetPoint"></param>
    /// <param name="closestPointInMovementSegment"></param>
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