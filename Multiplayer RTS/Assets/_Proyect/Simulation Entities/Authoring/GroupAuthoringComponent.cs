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

    public float destinationReachedDistance = 0.05f;

    public Behaviour behaviour;

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
        dstManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsToRefreshParentPath, TurnsWithoutRefresh = 0 });

        //selection
        dstManager.AddComponentData<Selectable>(entity, new Selectable());

        //collider
        dstManager.AddComponentData<Collider>(entity, new Collider() 
        { 
            Radious = (Fix64)0.5,
            Layer = ColliderLayer.GROUP
        });

        //target find
        dstManager.AddComponentData<SightRange>(entity, new SightRange() { Value = (Fix64)sightRange });
        dstManager.AddComponentData<ActTargetFilters>(entity, new ActTargetFilters()
        {
            ActOnEnemies = actOnEnemyTeam,
            ActOnTeamates = actOnTeamates
        });

        dstManager.AddBuffer<BEPosibleTarget>(entity);
        //Target AI
        dstManager.AddComponentData<GroupBehaviour>(entity, new GroupBehaviour() 
        {
            Value = behaviour
        });
        

        //team
        dstManager.AddComponentData<Team>(entity, new Team() { Number = team});

        //movement
        dstManager.AddComponentData<MovementState>(entity, new MovementState()
        {
            HexOcuppied = hexPos.Round(),
            PreviousStepDestiantionReached = false,

            DestinationReached = false,
            DestinationIsReachedDistance = (Fix64)destinationReachedDistance
        });
        //  steering
        dstManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget() { TargetPosition = hexPos, StopAtTarget = false });
        dstManager.AddComponentData<DesiredMovement>(entity, new DesiredMovement());

        //commands
        dstManager.AddComponentData<Commandable>(entity, new Commandable() { DeafaultCommand = CommandType.MOVE_COMMAND });
        dstManager.AddComponentData<CommandableDeathFlag>(entity, new CommandableDeathFlag());

        
        dstManager.AddComponentData<Speed>(entity, new Speed() { Value = (Fix64)parentSpeed });
        dstManager.AddComponentData<DestinationHex>(entity, new DestinationHex() { FinalDestination = hexPos.Round() });



        dstManager.AddComponentData<TriggerPathfinding>(entity, new TriggerPathfinding() { Destination = hexPos.Round() });

        //dstManager.AddComponentData<TPAtMouseClick>(entity, new TPAtMouseClick());
        Debug.Log("converting the parent entity");


        parentUnitLink.ParentEntityCreatedCallback(entity);
    }
}
