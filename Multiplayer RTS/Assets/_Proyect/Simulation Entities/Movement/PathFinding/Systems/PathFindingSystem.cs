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
public class PathFindingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        ActiveMap activeMap = MapManager.ActiveMap;        
        if (activeMap == null){ return; }
        Dictionary<Entity, List<Hex>> Paths = new Dictionary<Entity, List<Hex>>();

        //clean buffer and restart waypoint
        Entities.WithAll<PathWaypoint, TriggerPathfinding>().ForEach((Entity entity, ref PathWaypointIndex waypointIndex) => 
        {
            DynamicBuffer<PathWaypoint> buffer = World.EntityManager.GetBuffer<PathWaypoint>(entity);
            buffer.Clear();

            waypointIndex = new PathWaypointIndex() { Value = 0 };
        });

        //shortestPath
        Entities.ForEach((Entity entity, ref TriggerPathfinding pathSolicitude, ref HexPosition hexPosition) => 
        {
            var path = new List<Hex>(); 

            Hex startHex = hexPosition.HexCoordinates.Round();
            Hex destinationHex = pathSolicitude.Destination;
            if (!activeMap.map.MovementMapValues.TryGetValue(destinationHex, out bool c))
            {                
                destinationHex = MapUtilities.FindClosestOpenHex((FractionalHex)destinationHex, activeMap.map, true);
            }
            else if (!c)
            {
                destinationHex = MapUtilities.FindClosestOpenHex((FractionalHex)destinationHex, activeMap.map, true);
            }
            if (! activeMap.map.MovementMapValues.TryGetValue(startHex, out bool b))
            {                
                //it adds the start hex as the first waypoint in the path
                startHex = MapUtilities.FindClosestOpenHex(hexPosition.HexCoordinates, activeMap.map, true);
                path.Add(startHex);
            }
            
            
            if (startHex == destinationHex)
            {
                path.Add(startHex);
                Paths.Add(entity, path);
                return;
            }
            var startNode = new PathfindingNode(startHex, 0, 0, startHex);
            

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

                //enters here when the path have been found
                if (currentNode.Equals(destinationHex))
                {
                    var reversePath = new List<Hex>();
                    while (currentNode != startNode)
                    {
                        reversePath.Add(currentNode.hex);
                        currentNode = closedList[closedList.IndexOf(currentNode.parent)];
                    }
                    reversePath.Reverse();

                    path.AddRange(reversePath);
                    Paths.Add(entity, path);

                    openList.Dispose();
                    closedList.Dispose();
                    return;
                }

                for (int i = 0; i < 6; i++)
                {
                    var neightbor = currentNode.hex.Neightbor(i);
                    if (activeMap.map.MovementMapValues.ContainsKey(neightbor))
                    {
                        if (!activeMap.map.MovementMapValues[neightbor] || closedList.Contains(neightbor))
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


            //we may end up here if the destinaation isn't recheable from the start
            //a posible solution is to return the path to the closest reachable hex to the destination
            Debug.LogError($"The pathfinding was a failure for of index:{entity.Index}. The start node is: {startHex} and is open:{activeMap.map.MovementMapValues[startHex]}. the end node is: {destinationHex} and is open:{activeMap.map.MovementMapValues[destinationHex]}");
            openList.Dispose();
            closedList.Dispose();
        });
        
        //clean the buffer and then add the path there
        Entities.WithAll<PathWaypoint>().ForEach((Entity entity, ref TriggerPathfinding pathSolicitude) => 
        {
            DynamicBuffer<PathWaypoint> buffer = World.EntityManager.GetBuffer<PathWaypoint>(entity);
            buffer.Clear();

            List<Hex> path;
            Debug.Assert(Paths.TryGetValue(entity, out path), "the entity doesn't have a path. this is because the pathfinding failed before");

            
            foreach (Hex waypoint in path)
            {
                buffer.Add(new PathWaypoint() { Value = waypoint });
            }
        });

        //remove path solicitude
        Entities.ForEach((Entity entity, ref TriggerPathfinding pathSolicitude) => 
        {
            PostUpdateCommands.RemoveComponent<TriggerPathfinding>(entity);
        });

    }
}

#region Job stuff
//private EntityQuery pathingEntityQuerry;
//private EndSimulationEntityCommandBufferSystem endSimulationECB;

//private List<NativeHashMap<Hex, bool>> travesableMap = new List<NativeHashMap<Hex, bool>>();
//private List<NativeMultiHashMap<Entity, PathfindingNode>> pathfindingPaths = new List<NativeMultiHashMap<Entity, PathfindingNode>>();

//protected override void OnCreate()
//{
//    CleanHashMaps();
//    pathingEntityQuerry = GetEntityQuery(typeof(PathWaypoint), typeof(TriggerPathfinding), ComponentType.ReadOnly<HexPosition>());
//    endSimulationECB = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//}


//struct FindShortestPathJob : IJobForEachWithEntity<TriggerPathfinding, HexPosition>
//{
//    public NativeList<PathfindingNode> PathNodes;
//    public NativeMultiHashMap<Entity, PathfindingNode> Path;
//    [ReadOnly] public NativeHashMap<Hex, bool> Map;


//    public void Execute(Entity entity, int index, [ReadOnly]ref TriggerPathfinding pathSolicitude, [ReadOnly]ref HexPosition hexPosition)
//    {
//        Hex startHex = hexPosition.HexCoordinates.Round();
//        var startNode = new PathfindingNode(startHex, 0, 0, startHex);
//        var destinationHex = pathSolicitude.Destination;

//        var openList = new NativeList<PathfindingNode>(Allocator.Temp);
//        var closedList = new NativeList<PathfindingNode>(Allocator.Temp);

//        openList.Add(startNode);
//        while (openList.Length > 0)
//        {
//            var currentNode = openList[0];
//            for (int i = 1; i < openList.Length; i++)
//            {
//                var scanNode = openList[i];
//                if (currentNode.fCost > scanNode.fCost || currentNode.fCost == scanNode.fCost && currentNode.hCost > scanNode.hCost)
//                {
//                    currentNode = scanNode;
//                }
//            }
//            openList.RemoveAtSwapBack(openList.IndexOf(currentNode));
//            closedList.Add(currentNode);

//            if (currentNode.Equals(destinationHex))
//            {
//                int lenght = 0;
//                //enters here when the path have been found
//                while (currentNode != startNode)
//                {
//                    lenght++;
//                    Path.Add(entity, currentNode);
//                    currentNode = closedList[closedList.IndexOf(currentNode.parent)];
//                }
//                return;
//            }

//            for (int i = 0; i < 6; i++)
//            {
//                var neightbor = currentNode.hex.Neightbor(i);
//                if (Map.ContainsKey(neightbor))
//                {
//                    if (!Map[neightbor] || closedList.Contains(neightbor))
//                    {
//                        continue;
//                    }

//                    if (!openList.Contains(neightbor))
//                    {
//                        var neightborNode = new PathfindingNode(neightbor, currentNode.gCost + 1, neightbor.Distance(destinationHex), currentNode.hex);
//                        openList.Add(neightborNode);
//                    }
//                    else
//                    {
//                        int indexOfNeightbor = openList.IndexOf(neightbor);
//                        var neightborNode = openList[indexOfNeightbor];

//                        int newMovementCostToNeighbor = currentNode.gCost + 1;
//                        if (newMovementCostToNeighbor < neightborNode.gCost)
//                        {
//                            openList[indexOfNeightbor] = new PathfindingNode(neightbor, newMovementCostToNeighbor, neightbor.Distance(destinationHex), currentNode.hex);
//                        }
//                    }
//                }
//            }
//        }

//        openList.Dispose();
//        closedList.Dispose();
//    }
//}

//struct AddWaypointsToBufferJobChunk : IJobChunk
//{
//    [ReadOnly] public ArchetypeChunkEntityType entitiesType;
//    public ArchetypeChunkBufferType<PathWaypoint> waypointBufferType;
//    [ReadOnly] public NativeMultiHashMap<Entity, PathfindingNode> Path;

//    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//    {
//        var waypointBufferAccesor = chunk.GetBufferAccessor(waypointBufferType);
//        var entities = chunk.GetNativeArray(entitiesType);

//        for (int entityIndex = firstEntityIndex, i = 0; i < chunk.Count; i++, entityIndex++)
//        {
//            var entity = entities[i];
//            var buffer = waypointBufferAccesor[entityIndex];
//            buffer.Clear();


//            if (Path.TryGetFirstValue(entity, out PathfindingNode waypoint, out NativeMultiHashMapIterator<Entity> iterator))
//            {
//                do
//                {
//                    buffer.Add(new PathWaypoint() { Value = waypoint.hex });

//                } while (Path.TryGetNextValue(out waypoint, ref iterator));
//            }
//        }
//    }
//}



////    struct DestroyTriggerJob : IJobForEachWithEntity<TriggerPathfinding>
////    {
////        public EntityCommandBuffer ECB;
////        public void Execute(Entity entity, int index, ref TriggerPathfinding trigger)
////        {
////            ECB.RemoveComponent<TriggerPathfinding>(entity);
////        }
////    }

//protected override void OnStopRunning()
//{
//    CleanHashMaps();
//}

//protected override JobHandle OnUpdate(JobHandle inputDependencies)
//{
//    //        if (MapManager.ActiveMap == null) return inputDependencies;

//    CleanHashMaps();

//    var mapStaticValues = MapManager.ActiveMap.map.StaticMapValues;
//    int countOfEntitiesToProcess = pathingEntityQuerry.CalculateEntityCount();


//    var Paths = new NativeMultiHashMap<Entity, PathfindingNode>(countOfEntitiesToProcess * 8, Allocator.TempJob);

//    var Map = new NativeHashMap<Hex, bool>(mapStaticValues.Count, Allocator.TempJob);
//    foreach (var value in mapStaticValues)
//    {
//        if (!Map.TryAdd(value.Key, value.Value))
//        {
//            Debug.LogError("You cannot add more elements to the 'Map' native hash map. Something is wrong check it out");
//        }
//    }
//    pathfindingPaths.Add(Paths);
//    travesableMap.Add(Map);

//    //var bufferEntityConverter = GetBufferFromEntity<PathWaypoint>(false);


//    var pathFindingJob = new FindShortestPathJob()
//    {
//        Map = Map,
//        Path = Paths.AsParallelWriter()
//    }.Schedule(this, inputDependencies);
//    var addWaypointsToBufferJob = new AddWaypointsToBufferJobChunk()
//    {
//        entitiesType = GetArchetypeChunkEntityType(),
//        waypointBufferType = GetArchetypeChunkBufferType<PathWaypoint>(),
//        Path = Paths
//    }.Schedule(pathingEntityQuerry, pathFindingJob);
//    return addWaypointsToBufferJob;
//    //        travesableMap.Add(Map);
//    //        pathfindingPaths.Add(Paths);

//    //        var destroyTriggerJob = new DestroyTriggerJob()
//    //        {
//    //            ECB = endSimulationECB.CreateCommandBuffer()
//    //        }.ScheduleSingle(this, addWaypointsToBufferJob);
//    //        endSimulationECB.AddJobHandleForProducer(destroyTriggerJob);


//    //        return destroyTriggerJob;
//    return inputDependencies;
//}

//private void CleanHashMaps()
//{
//    foreach (var item in travesableMap)
//    {
//        item.Dispose();
//    }
//    foreach (var item in pathfindingPaths)
//    {
//        item.Dispose();
//    }
//    travesableMap.Clear();
//    pathfindingPaths.Clear();
//}
#endregion