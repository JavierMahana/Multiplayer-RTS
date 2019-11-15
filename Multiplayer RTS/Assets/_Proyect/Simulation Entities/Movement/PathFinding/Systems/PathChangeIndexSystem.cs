using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
//this system is in charge of changing the index of the pathfinding buffer.
//it depends on the pathfinding and the Target selection depends on it.
public class PathChangeIndexSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //que pasa si es que la entidad no le importa su index de path?... ahora nada solo se va aumentando si llega a suficiente distancia del waypoint y 
        Entities.WithAll<PathWaypoint>().ForEach(
            (Entity entity, ref HexPosition position, ref PathWaypointIndex pathWaypointIndex, ref WaypointReachedDistance waypointReachedDistance) => 
        {
            var buffer = World.EntityManager.GetBuffer<PathWaypoint>(entity);
            if (buffer.Length == 0 || buffer.Length <= pathWaypointIndex.Value + 1) return;
            else 
            {
                var waypoint = buffer[pathWaypointIndex.Value];
                var distance = position.HexCoordinates.Distance((FractionalHex)waypoint.Value);
                if (distance <= waypointReachedDistance.Value) 
                {
                    pathWaypointIndex.Value += 1; 
                } 
            }
        });
    }
}