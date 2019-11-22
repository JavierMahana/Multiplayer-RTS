using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using FixMath.NET;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

//[DisableAutoCreation]
////obsoleta
//public class PathFollowSystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        //change waypoint if the current one is reached
//        Entities.WithAll<PathWaypoint>().ForEach((Entity entity,ref PathWaypointIndex waypointIndex, ref WaypointReachedDistance waypointReachedDistance, ref HexPosition hexPosition) => 
//        {
//            var buffer = World.EntityManager.GetBuffer<PathWaypoint>(entity);
//            if (buffer.Length == 0) { return; }

//            Hex targetWaypoint = buffer[waypointIndex.Value];

//            var distance = hexPosition.HexCoordinates.Distance((FractionalHex)targetWaypoint);
//            if (distance <= waypointReachedDistance.Value)
//            {
//                if (buffer.Length <= waypointIndex.Value + 1)
//                {
//                    //the final waypoint have been reached.
//                    return;
//                }
//                waypointIndex = new PathWaypointIndex() { Value = waypointIndex.Value + 1 };
//            }
//        });
       

//        //move towards next waypoint
//        Entities.WithAll<PathWaypoint>().ForEach((Entity entity, ref HexPosition hexPosition, ref PathWaypointIndex waypointIndex, ref Speed speed) => 
//        {
//            var buffer = World.EntityManager.GetBuffer<PathWaypoint>(entity);
//            if (buffer.Length == 0) { return; }

//            Hex targetWaypoint = buffer[waypointIndex.Value];
//            FractionalHex currentPos = hexPosition.HexCoordinates;

//            var distanceVector = (FractionalHex)targetWaypoint - currentPos;
//            var direction = distanceVector / distanceVector.Magnitude();
//            var newPosition = currentPos + (direction * MainSimulationLoopSystem.SimulationDeltaTime * speed.Value);

//            hexPosition = new HexPosition() { HexCoordinates = newPosition };
        
//        });        
//    }
//}

#region job stuff



//struct PathFollowJob : IJobForEachWithEntity<HexPosition, PathWaypointIndex, Speed>
//{
//    public Fix64 deltaTime;

//    [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;
//    public void Execute(Entity entity, int index, ref HexPosition hexPos, [ReadOnly]ref PathWaypointIndex waypointIndex, [ReadOnly] ref Speed speed)
//    {

//    }
//}
//[RequireComponentTag(typeof(PathWaypoint))]
//struct PathFollowJob : IJobForEachWithEntity<HexPosition, PathWaypointIndex, Speed>
//{
//    public Fix64 deltaTime;

//    [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;
//    public void Execute(Entity entity, int index, ref HexPosition hexPos, [ReadOnly]ref PathWaypointIndex waypointIndex, [ReadOnly] ref Speed speed)
//    {
//        var buffer = bufferAccess[entity];
//        if (buffer.Length == 0) { return; }

//        Hex targetWaypoint = buffer[waypointIndex.Value];
//        FractionalHex currentPos = hexPos.HexCoordinates;

//        var distanceVector = (FractionalHex)targetWaypoint - currentPos;
//        var direction = distanceVector / distanceVector.Magnitude();
//        var newPosition = currentPos + (direction * deltaTime * speed.Value);

//        hexPos = new HexPosition() { HexCoordinates = newPosition };
//    }
//}
//[RequireComponentTag(typeof(PathWaypoint))]
//struct UpdateWaypointJob : IJobForEachWithEntity<PathWaypointIndex, WaypointReachedDistance, HexPosition>
//{
//    [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;

//    public void Execute(Entity entity, int index, ref PathWaypointIndex waypointIndex, [ReadOnly] ref WaypointReachedDistance waypointReachedDistance, [ReadOnly] ref HexPosition hexPos)
//    {

//        var buffer = bufferAccess[entity];
//        if (buffer.Length == 0) { return; }

//        Hex targetWaypoint = buffer[waypointIndex.Value];

//        var distance = hexPos.HexCoordinates.Distance((FractionalHex)targetWaypoint);
//        if (distance <= waypointReachedDistance.Value)
//        {
//            if (buffer.Length <= waypointIndex.Value + 1)
//            {
//                //the final waypoint have been reached.
//                //right now we just return.
//                return;
//            }
//            waypointIndex = new PathWaypointIndex() { Value = waypointIndex.Value + 1 };
//        }
//    }
//}

//struct UpdateWaypointJobChunk : IJobChunk
//{
//    [ReadOnly] public ArchetypeChunkBufferType<PathWaypoint> waypointBufferType;
//    [ReadOnly] public ArchetypeChunkComponentType<PathWaypointIndex> waypointIndexType;
//    [ReadOnly] public ArchetypeChunkComponentType<HexPosition> hexPositionType;
//    [ReadOnly] public ArchetypeChunkComponentType<WaypointReachedDistance> waypointReachedDistanceType;

//    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//    {
//        var waypointBufferAccessor = chunk.GetBufferAccessor(waypointBufferType);
//        var waypointIndices = chunk.GetNativeArray(waypointIndexType);
//        var hexPositions = chunk.GetNativeArray(hexPositionType);
//        var waypointReachedDistances = chunk.GetNativeArray(waypointReachedDistanceType);

//        for (int entityIndex = firstEntityIndex, i = 0; i < chunk.Count; i++, entityIndex++)
//        {
//            var waypointReachedDistance = waypointReachedDistances[i];
//            var hexPos = hexPositions[i];
//            var waypointIndex = waypointIndices[i];
//            var buffer = waypointBufferAccessor[entityIndex];

//            if (buffer.Length == 0) { return; }

//            Hex targetWaypoint = buffer[waypointIndex.Value];

//            var distance = hexPos.HexCoordinates.Distance((FractionalHex)targetWaypoint);
//            if (distance <= waypointReachedDistance.Value)
//            {
//                if (buffer.Length <= waypointIndex.Value + 1)
//                {
//                    //the final waypoint have been reached.
//                    //right now we just return.
//                    return;
//                }
//                waypointIndex = new PathWaypointIndex() { Value = waypointIndex.Value + 1 };
//            }
//        }
//    }
//}

//protected override JobHandle OnUpdate(JobHandle inputDependencies)
//{
//    var followPathForechJob = new PathFollowJob()
//    {
//        deltaTime = (Fix64)Time.deltaTime,
//        bufferAccess = GetBufferFromEntity<PathWaypoint>(true)
//    };
//    var updateWaypointsJob = new UpdateWaypointJobChunk()
//    {
//        hexPositionType = GetArchetypeChunkComponentType<HexPosition>(true),
//        waypointBufferType = GetArchetypeChunkBufferType<PathWaypoint>(true),
//        waypointIndexType = GetArchetypeChunkComponentType<PathWaypointIndex>(true),
//        waypointReachedDistanceType = GetArchetypeChunkComponentType<WaypointReachedDistance>(true)
//    }.Schedule(activePathQuerry, inputDependencies);
//    //var updateWaypointsJob = new UpdateWaypointJob()
//    //{
//    //    bufferAccess = GetBufferFromEntity<PathWaypoint>(true),
//    //};



//    return followPathForechJob.Schedule(this, updateWaypointsJob);
//}

//EntityQuery activePathQuerry;
//protected override void OnCreate()
//{
//    activePathQuerry = GetEntityQuery(typeof(PathWaypoint), typeof(PathWaypointIndex), typeof(WaypointReachedDistance), typeof(HexPosition));
//}
#endregion


//struct Test : IJobForEach<Speed>
//{           
//    public void Execute([ReadOnly]ref Speed t)
//    {

//        var openList = new NativeList<Hex>(Allocator.Temp);
//        var closedList = new NativeList<int>(Allocator.Temp);

//        var a = new Hex(1, 1);
//        var b = new Hex(1, 2);
//        var c = new Hex(1, 3);
//        Debug.Log($"lenght after add {openList.Length}");
//        openList.Add(a);
//        openList.Add(b);
//        openList.Add(c);
//        int i = openList.IndexOf(b);

//        Debug.Log($"lenght before add {openList.Length}. index of (1,1) ={openList.IndexOf(a)} (1,2) = {i}");
//        openList.RemoveAtSwapBack(openList.IndexOf(a));
//        Debug.Log($"lenght before remove {openList.Length}. (1,1,-2) was removed. element of the index 0 is = {openList[0]}");


//        openList.Dispose();
//        closedList.Dispose();
//    }
//}