using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using FixMath.NET;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class UnitAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public float waypointReachedDistance;
    public float speed;
    public Transform[] waypoints;


    public bool log;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }




        Layout layout = MapManager.ActiveMap.layout;
        if (log) Debug.Log($"transform.position; {(Fix64)transform.position.x}, {(Fix64)transform.position.y} ", this);
        var fractionalHex = layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));
        if (log) Debug.Log($"hexagonal pos; {fractionalHex.q}|{fractionalHex.r}|{fractionalHex.s}");
        var pos = layout.HexToWorld(fractionalHex);
        if (log) Debug.Log($"world pos converting from hexagonal; {pos.x}, {pos.y} ", this);



        dstManager.AddComponentData(entity, new HexPosition() { HexCoordinates = fractionalHex });
        var buffer = dstManager.AddBuffer<PathWaypoint>(entity);
        foreach (var waypoint in waypoints)
        {
            var waypointPostion = (Vector2)waypoint.position;
            Hex waypointHex = layout.WorldToHex(new FixVector2((Fix64)waypointPostion.x, (Fix64)waypointPostion.y));
            buffer.Add(new PathWaypoint() { Value = waypointHex });
        }
        dstManager.AddComponentData(entity, new PathWaypointIndex() { Value = 0 });
        dstManager.AddComponentData(entity, new Speed() { Value = (Fix64)speed });
        dstManager.AddComponentData(entity, new WaypointReachedDistance() { Value = (Fix64)waypointReachedDistance });
    }
}
