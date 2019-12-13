using FixMath.NET;
using Javier.RTS;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Photon.Pun;

[RequiresEntityConversion]
public class GroupAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public int team = 0;

    public AssigningUnitParentAuthoring parentUnitLink;
    public int turnsToRefreshParentPath;
    public float parentSpeed;
    public float parentWaypointReachedDistance;

    public bool actOnEnemyTeam;
    public bool actOnTeamates;
    public float sightRange;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }

        Layout layout = MapManager.ActiveMap.layout;
        var hexPos = layout.WorldToFractionalHex((FixVector2)transform.position);


        dstManager.AddComponentData<Group>(entity, new Group());
        dstManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = hexPos });
        dstManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });

        //pathfinding
        dstManager.AddBuffer<PathWaypoint>(entity);
        dstManager.AddComponentData<PathWaypointIndex>(entity, new PathWaypointIndex() { Value = 0 });
        dstManager.AddComponentData<WaypointReachedDistance>(entity, new WaypointReachedDistance() { Value = (Fix64)parentWaypointReachedDistance });

        //selection
        dstManager.AddComponentData<Selectable>(entity, new Selectable());

        //collider
        dstManager.AddComponentData<Collider>(entity, new Collider() 
        { 
            Radious = (Fix64)0.5,
            Layer = ColliderLayer.GROUP
        });

        //Target AI
        dstManager.AddBuffer<BEPosibleTarget>(entity);
        dstManager.AddComponentData<GroupAI>(entity, new GroupAI() { ArePossibleTargets = false });
        dstManager.AddComponentData<GroupBehaviour>(entity, new GroupBehaviour() 
        {
            ActOnEnemies = actOnEnemyTeam,
            ActOnTeamates = actOnTeamates,

            SightDistance = (Fix64)sightRange
        });
        

        //team
        dstManager.AddComponentData<Team>(entity, new Team() { Number = team});
    

        //steering
        dstManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget() { TargetPosition = hexPos, StopAtTarget = false });
        dstManager.AddComponentData<DesiredMovement>(entity, new DesiredMovement());

        //commands
        dstManager.AddComponentData<Commandable>(entity, new Commandable() { DeafaultCommand = CommandType.MOVE_COMMAND});
        dstManager.AddComponentData<CommandableDeathFlag>(entity, new CommandableDeathFlag());

        dstManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsToRefreshParentPath, TurnsWithoutRefresh = 0 });        
        dstManager.AddComponentData<Speed>(entity, new Speed() { Value = (Fix64)parentSpeed });        
        dstManager.AddComponentData<DestinationHex>(entity, new DestinationHex() { Value = hexPos.Round() });

        //dstManager.AddComponentData<TPAtMouseClick>(entity, new TPAtMouseClick());
        dstManager.AddComponent<debugTarget>(entity);
        Debug.Log("converting the parent entity");


        parentUnitLink.ParentEntityCreatedCallback(entity);
    }
}
