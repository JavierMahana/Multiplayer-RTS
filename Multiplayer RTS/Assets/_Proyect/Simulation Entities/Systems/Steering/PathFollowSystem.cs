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
    private bool log = true;
    private EntityQuery updateWaypointsQuerry;
    private EntityQuery followPathQuerry;

    struct PathFollowJob : IJobChunk
    {
        public bool log;
        public Fix64 deltaTime;

        [ReadOnly] public ArchetypeChunkBufferType<PathWaypoint> BufferType;
        public ArchetypeChunkComponentType<PathWaypointIndex> WaypointIndexType;
        [ReadOnly] public ArchetypeChunkComponentType<Speed> SpeedType;
        public ArchetypeChunkComponentType<HexPosition> HexPositionType;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var positions = chunk.GetNativeArray(HexPositionType);
            var waypointIndices = chunk.GetNativeArray(WaypointIndexType);
            var speeds = chunk.GetNativeArray(SpeedType);
            var lookup = chunk.GetBufferAccessor(BufferType);
            for (int i = firstEntityIndex, j = 0; j < chunk.Count; i++, j++)
            {
                var buffer = lookup[i];
                Hex targetWaypoint = buffer[waypointIndices[j].Value];
                FractionalHex currentPos = positions[j].HexCoordinates;

                //if (log) Debug.Log($"the current waypoint index is : {waypointIndices[j].Value}");
                //if (log) Debug.Log($"and the waypoint it points is : {targetWaypoint.q}|{targetWaypoint.r}|{targetWaypoint.s}");
                //if (log) Debug.Log($"Also, the entity hex postion is : {currentPos.q}|{currentPos.r}|{currentPos.s}");

                var distanceVector = (FractionalHex)targetWaypoint - currentPos;
                var direction = distanceVector / distanceVector.Magnitude();
                var newPosition = currentPos + (direction * deltaTime * speeds[j].Value);

                positions[j] = new HexPosition() { HexCoordinates = newPosition} ;
            }
        }
    }
    struct UpdateWaypointJob : IJobChunk
    {
        public bool log;

        [ReadOnly] public ArchetypeChunkBufferType<PathWaypoint> WaypointBufferType;
        [ReadOnly] public ArchetypeChunkComponentType<WaypointReachedDistance> WaypointDistanceType;
        public ArchetypeChunkComponentType<PathWaypointIndex> WaypointIndexType;
        [ReadOnly] public ArchetypeChunkComponentType<HexPosition> HexPositionType;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var bufferAccessor = chunk.GetBufferAccessor(WaypointBufferType);

            var waypointRadiuses = chunk.GetNativeArray(WaypointDistanceType);
            var waypointIndices = chunk.GetNativeArray(WaypointIndexType);            
            var positions = chunk.GetNativeArray(HexPositionType);
            
            for (int i = firstEntityIndex, j = 0; j < chunk.Count; i++, j++)
            {
                FractionalHex currentPos = positions[j].HexCoordinates;
                var buffer = bufferAccessor[i];
                var waypointIndex = waypointIndices[j];
                Hex targetWaypoint = buffer[waypointIndex.Value];

                //if (log) Debug.Log($"the current waypoint index is : {waypointIndex.Value}");
                //if (log) Debug.Log($"and the waypoint it points is : {targetWaypoint.q}|{targetWaypoint.r}|{targetWaypoint.s}");
                //if (log) Debug.Log($"Also, the entity hex postion is : {currentPos.q}|{currentPos.r}|{currentPos.s}");
                //var roundedPos = currentPos.Round();
                //if (log) Debug.Log($"and the entity rounded hex postion is : {roundedPos.q}|{roundedPos.r}|{roundedPos.s}");

                var waypointRadius = waypointRadiuses[j].Value;

                var distance = currentPos.Distance((FractionalHex)targetWaypoint);
                if (distance <= waypointRadius)
                {
                    if (buffer.Length <= waypointIndex.Value + 1)
                    {
                        //the final waypoint have been reached.
                        //right now we just return.
                        return;
                    }
                    waypointIndices[j] = new PathWaypointIndex() { Value = waypointIndex.Value + 1 };
                }
            }
        }
    }

    protected override void OnCreate()
    {
        followPathQuerry = GetEntityQuery(
            typeof(PathWaypointIndex), typeof(HexPosition), ComponentType.ReadOnly<Speed>(), ComponentType.ReadOnly<PathWaypoint>());
        updateWaypointsQuerry = GetEntityQuery(
            ComponentType.ReadOnly<PathWaypoint>(), ComponentType.ReadOnly<WaypointReachedDistance>(), typeof(PathWaypointIndex), ComponentType.ReadOnly<HexPosition>());
    }
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var followPathJob = new PathFollowJob()
        {
            log = log,
            deltaTime = (Fix64)Time.deltaTime,

            BufferType = GetArchetypeChunkBufferType<PathWaypoint>(true),

            WaypointIndexType = GetArchetypeChunkComponentType<PathWaypointIndex>(),
            SpeedType = GetArchetypeChunkComponentType<Speed>(true),
            HexPositionType = GetArchetypeChunkComponentType<HexPosition>()
        };

        var updateWaypointsJob = new UpdateWaypointJob()
        {
            log = log,

            WaypointBufferType = GetArchetypeChunkBufferType<PathWaypoint>(true),
            WaypointDistanceType = GetArchetypeChunkComponentType<WaypointReachedDistance>(true),
            WaypointIndexType = GetArchetypeChunkComponentType<PathWaypointIndex>(),
            HexPositionType = GetArchetypeChunkComponentType<HexPosition>(true)
        };

        updateWaypointsJob.Run(updateWaypointsQuerry);
        followPathJob.Run(followPathQuerry);
        return inputDependencies;
    }
}