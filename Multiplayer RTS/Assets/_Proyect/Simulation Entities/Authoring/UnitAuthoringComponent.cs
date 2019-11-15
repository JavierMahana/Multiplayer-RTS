using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using FixMath.NET;

[DisallowMultipleComponent]
[RequiresEntityConversion]
//starts on reinforcement mode by default
public class UnitAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public float waypointReachedDistance;
    public float speed;
    public int turnsTorefreshPath;
    public Transform group;
    public bool destroyParentGO;

    public bool log;
    private void InitUnitEntity(Entity entity, Layout layout, EntityManager dstManager, Entity parent)
    {
        var fractionalHex = layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));
        var pos = layout.HexToWorld(fractionalHex);




        

        dstManager.AddComponentData(entity, new HexPosition() { HexCoordinates = fractionalHex });
        dstManager.AddSharedComponentData<Parent>(entity, new Parent() { ParentEntity = parent });
        dstManager.AddComponentData<OnReinforcement>(entity, new OnReinforcement());
        dstManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsTorefreshPath, TurnsWithoutRefresh = 0 });
        dstManager.AddComponentData(entity, new PathWaypointIndex() { Value = 0 });
        var buffer = dstManager.AddBuffer<PathWaypoint>(entity);
        dstManager.AddComponentData(entity, new Speed() { Value = (Fix64)speed });
        dstManager.AddComponentData(entity, new WaypointReachedDistance() { Value = (Fix64)waypointReachedDistance });
        dstManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });
        dstManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget());

        //dstManager.AddComponentData(entity, new MouseClickTriggerPathfinding());
    }
    private Entity InitGroupEntity(Layout layout, EntityManager dstManager)
    {
        var entity = dstManager.CreateEntity();

        var worldPos = group.position;
        var hexPos = layout.WorldToFractionalHex(new FixVector2((Fix64)worldPos.x, (Fix64)worldPos.y));

        dstManager.AddComponentData<Group>(entity, new Group());
        dstManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = hexPos });
        dstManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });

        
        return entity;
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }




        Layout layout = MapManager.ActiveMap.layout;

        var parentEntity = InitGroupEntity(layout, dstManager);
        InitUnitEntity(entity, layout, dstManager, parentEntity);

        if(destroyParentGO)Destroy(group.gameObject);
    }
}
