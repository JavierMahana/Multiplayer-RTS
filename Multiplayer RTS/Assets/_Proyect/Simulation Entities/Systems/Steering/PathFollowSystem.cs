using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using FixMath.NET;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class PathFollowSystem : JobComponentSystem
{

    [RequireComponentTag(typeof(PathWaypoint))]
    struct PathFollowJob : IJobForEachWithEntity<HexPosition, PathWaypointIndex, Speed>
    {
        public Fix64 deltaTime;

        [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;
        public void Execute(Entity entity, int index, ref HexPosition hexPos, [ReadOnly]ref PathWaypointIndex waypointIndex, [ReadOnly] ref Speed speed)
        {
            var buffer = bufferAccess[entity];
            Hex targetWaypoint = buffer[waypointIndex.Value];
            FractionalHex currentPos = hexPos.HexCoordinates;

            var distanceVector = (FractionalHex)targetWaypoint - currentPos;
            var direction = distanceVector / distanceVector.Magnitude();
            var newPosition = currentPos + (direction * deltaTime * speed.Value);

            hexPos = new HexPosition() { HexCoordinates = newPosition };
        }
    }
    [RequireComponentTag(typeof(PathWaypoint))]
    struct UpdateWaypointJob : IJobForEachWithEntity<PathWaypointIndex, WaypointReachedDistance, HexPosition>
    {
        [ReadOnly] public BufferFromEntity<PathWaypoint> bufferAccess;
        public void Execute(Entity entity, int index, ref PathWaypointIndex waypointIndex, [ReadOnly] ref WaypointReachedDistance waypointReachedDistance, [ReadOnly] ref HexPosition hexPos)
        {
            var buffer = bufferAccess[entity];
            Hex targetWaypoint = buffer[waypointIndex.Value];

            var distance = hexPos.HexCoordinates.Distance((FractionalHex)targetWaypoint);
            if (distance <= waypointReachedDistance.Value)
            {
                if (buffer.Length <= waypointIndex.Value + 1)
                {
                    //the final waypoint have been reached.
                    //right now we just return.
                    return;
                }
                waypointIndex = new PathWaypointIndex() { Value = waypointIndex.Value + 1 };
            }
        }
    }
  

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var followPathForechJob = new PathFollowJob()
        {
            deltaTime = (Fix64)Time.deltaTime,
            bufferAccess = GetBufferFromEntity<PathWaypoint>(true)
        };
        var updateWaypointsJob = new UpdateWaypointJob()
        {
            bufferAccess = GetBufferFromEntity<PathWaypoint>(true),
        };
       
        updateWaypointsJob.Run(this);
        followPathForechJob.Run(this);

        return inputDependencies;
    }
}




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