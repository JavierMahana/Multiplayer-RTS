using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class PathFindingSystem : JobComponentSystem
{
    //quizas hacer algo antes de hacer el a*

    private EntityQuery pathingEntityQuerry;
    protected override void OnCreate()
    {
        pathingEntityQuerry = GetEntityQuery(typeof(PathWaypoint), typeof(TriggerPathfinding), ComponentType.ReadOnly<HexPosition>());
    }
    struct FindShortestPathJob : IJobForEachWithEntity<TriggerPathfinding, HexPosition>
    {
        //this multihashmap must be initialized in the main thread
        public NativeHashMap<Entity, NativeList<PathfindingNode>> Path;
        [ReadOnly] public NativeHashMap<Hex, bool> Map;


        public void Execute(Entity entity, int index, [ReadOnly]ref TriggerPathfinding pathSolicitude, [ReadOnly]ref HexPosition hexPosition)
        {
            Hex startHex = hexPosition.HexCoordinates.Round();
            var startNode = new PathfindingNode(startHex, 0, 0, startHex);
            var destinationHex = pathSolicitude.Destination;

            var openList = new NativeList<PathfindingNode>(Allocator.Temp);
            var closedList = new NativeList<PathfindingNode>(Allocator.Temp);

            openList.Add(startNode);
            while (openList.Length > 0)
            {
                var currentNode = openList[0];
                for (int i = 1; i < openList.Length; i++)
                {
                    var scanNode = openList[i];
                    if (currentNode.fCost > scanNode.fCost || currentNode.fCost == scanNode.fCost && currentNode.hCost > scanNode.hCost)
                    {
                        currentNode = scanNode;
                    }
                }
                openList.RemoveAtSwapBack(openList.IndexOf(currentNode));
                closedList.Add(currentNode);

                if (currentNode.Equals(destinationHex))
                {
                    RetraceAndAssignPath(entity, startNode, currentNode, closedList);
                    return;
                }

                for (int i = 0; i < 6; i++)
                {
                    var neightbor = currentNode.hex.Neightbor(i);
                    if (Map.ContainsKey(neightbor))
                    {
                        if (!Map[neightbor] || closedList.Contains(neightbor))
                        {
                            continue;
                        }

                        if (!openList.Contains(neightbor))
                        {
                            var neightborNode = new PathfindingNode(neightbor, currentNode.gCost + 1, neightbor.Distance(destinationHex), currentNode.hex);
                            openList.Add(neightborNode);
                        }
                        else 
                        {
                            int indexOfNeightbor = openList.IndexOf(neightbor);
                            var neightborNode = openList[indexOfNeightbor];

                            int newMovementCostToNeighbor = currentNode.gCost + 1;
                            if (newMovementCostToNeighbor < neightborNode.gCost)
                            {
                                openList[indexOfNeightbor] = new PathfindingNode(neightbor, newMovementCostToNeighbor, neightbor.Distance(destinationHex), currentNode.hex);
                            }
                        }
                    }
                }
            }

            openList.Dispose();
            closedList.Dispose();
        }
        private void RetraceAndAssignPath(Entity entity, PathfindingNode startingNode, PathfindingNode endingNode, NativeList<PathfindingNode> closedList)
        {
            var tempPath = new NativeList<PathfindingNode>(Allocator.Temp);
            var currentNode = endingNode;
            while (currentNode != startingNode)
            {
                tempPath.Add(currentNode);
                currentNode = closedList[closedList.IndexOf(currentNode.parent)];
            }

            var pathForThisEntity = Path[entity];
            pathForThisEntity.Clear();
            for (int i = tempPath.Length - 1; i >= 0; i--)
            {
                pathForThisEntity.Add(tempPath[i]);
            }

            Path[entity] = pathForThisEntity;

            tempPath.Dispose();
        }
    }
    struct SimplifyPath : IJobForEach<TriggerPathfinding, HexPosition>
    {
        public void Execute(ref TriggerPathfinding c0, ref HexPosition c1)
        {
            throw new System.NotImplementedException();
        }
    }
    struct AddWaypointsToBufferJob : IJobForEach<TriggerPathfinding, HexPosition>
    {
        [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;


        public void Execute(ref TriggerPathfinding c0, ref HexPosition c1)
        {
            throw new System.NotImplementedException();
        }
    }
    struct DestroyTriggerJob : IJobForEachWithEntity<TriggerPathfinding>
    {
        public EntityCommandBuffer ECB;
        public void Execute(Entity entity, int index, ref TriggerPathfinding trigger)
        {
            ECB.RemoveComponent<TriggerPathfinding>(entity);
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        return inputDependencies;
    }
}