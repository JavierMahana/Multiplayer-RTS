using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixMath.NET;
using Javier.RTS;


[DisallowMultipleComponent]
public abstract class BaseGroupAuthoringComponent : EntityAuthoringBase
{
    [SerializeField]
    protected int team = 0;


    [SerializeField]
    protected int turnsToRefreshParentPath = 10;
    [SerializeField]
    protected float parentSpeed = 2.2f;
    [SerializeField]
    protected float parentWaypointReachedDistance = 0.1f;

    [SerializeField]
    protected bool actOnEnemyTeam = false;
    [SerializeField]
    protected bool actOnTeamates = false;
    [SerializeField]
    protected ActType actType = ActType.ATTACK;

    [SerializeField]
    protected float sightRange = 2;

    [SerializeField]
    protected float destinationReachedDistance = 0.05f;

    [SerializeField]
    protected Behaviour behaviour = Behaviour.DEFAULT;

    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }

        Layout layout = MapManager.ActiveMap.layout;
        var hexPos = layout.WorldToFractionalHex((FixVector2)transform.position);


        entityManager.AddComponentData<Group>(entity, new Group());
        entityManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = hexPos });
        entityManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });

        //pathfinding
        entityManager.AddBuffer<PathWaypoint>(entity);
        entityManager.AddComponentData<PathWaypointIndex>(entity, new PathWaypointIndex() { Value = 0 });
        entityManager.AddComponentData<WaypointReachedDistance>(entity, new WaypointReachedDistance() { Value = (Fix64)parentWaypointReachedDistance });
        entityManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsToRefreshParentPath, TurnsWithoutRefresh = 0 });

        //selection
        entityManager.AddComponentData<Selectable>(entity, new Selectable());

        //collider
        entityManager.AddComponentData<Collider>(entity, new Collider()
        {
            Radius = (Fix64)0.5,
            Layer = ColliderLayer.GROUP

        });

        //target find
        entityManager.AddComponentData<SightRange>(entity, new SightRange() { Value = (Fix64)sightRange });
        entityManager.AddComponentData<ActTargetFilters>(entity, new ActTargetFilters()
        {
            ActOnEnemies = actOnEnemyTeam,
            ActOnTeamates = actOnTeamates,
            actType = actType
        });


        entityManager.AddBuffer<BEPosibleTarget>(entity);

        
        //Target AI
        entityManager.AddComponentData<GroupBehaviour>(entity, new GroupBehaviour()
        {
            Value = behaviour
        });


        //team
        entityManager.AddComponentData<Team>(entity, new Team() { Number = team });

        //movement
        entityManager.AddComponentData<MovementState>(entity, new MovementState()
        {
            HexOcuppied = hexPos.Round(),
            PreviousStepDestiantionReached = false,

            DestinationReached = false,
            DestinationIsReachedDistance = (Fix64)destinationReachedDistance
        });
        entityManager.AddComponentData<Speed>(entity, new Speed() { Value = (Fix64)parentSpeed });
        entityManager.AddComponentData<DestinationHex>(entity, new DestinationHex() { FinalDestination = MapUtilities.FindClosestOpenHex( hexPos, MapManager.ActiveMap.map, true) });


        //  steering
        entityManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget() { TargetPosition = hexPos, StopAtTarget = false });
        entityManager.AddComponentData<DesiredMovement>(entity, new DesiredMovement());

        //commands
        entityManager.AddComponentData<Commandable>(entity, new Commandable() { DeafaultCommand = CommandType.MOVE_COMMAND });
        entityManager.AddComponentData<CommandableDeathFlag>(entity, new CommandableDeathFlag());



        entityManager.AddComponentData<TriggerPathfinding>(entity, new TriggerPathfinding() { Destination = hexPos.Round() });

        //dstManager.AddComponentData<TPAtMouseClick>(entity, new TPAtMouseClick());
        //Debug.Log("converting the parent entity");
    }
}
