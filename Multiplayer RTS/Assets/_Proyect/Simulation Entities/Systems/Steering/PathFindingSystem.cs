using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class PathFindingSystem : JobComponentSystem
{
    //quizas hacer algo antes de hacer el a*

    private EntityQuery pathingEntityQuerry;
    protected override void OnCreate()
    {
        pathingEntityQuerry = GetEntityQuery(typeof(PathWaypoint), typeof(TriggerPathfinding), ComponentType.ReadOnly<HexPosition>());
    }
    struct FindShortestPathJob : IJobChunk
    {
        public ArchetypeChunkBufferType<PathWaypoint> WaypointBufferType;
        public NativeArray<Hex> Path;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {


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
        //var job = new PathFindingSystemJob();
        //var triggerCleanupJob = new DestroyTriggerJob() { ECB =  EndSimulationEntityCommandBufferSystem }

        //return job.Schedule(this, inputDependencies);
        return inputDependencies;
    }
}