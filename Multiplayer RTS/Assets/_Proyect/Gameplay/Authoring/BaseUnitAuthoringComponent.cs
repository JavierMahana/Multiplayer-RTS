using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixMath.NET;

public abstract class BaseUnitAuthoringComponent : EntityAuthoringBase
{

    [SerializeField]
    private EntityFilter parentEntityFilter = null;

    [SerializeField]
    private int health = 80;


    [SerializeField]
    private float waypointReachedDistance = 0.1f;
    [SerializeField]
    private float speed = 2;
    [SerializeField]
    private int turnsTorefreshPath = 20;

    [SerializeField]
    private float radious = 0.1f;
    [SerializeField]
    private float collisionIntensity = 0.8f;
    [Range(0, 1)]
    [SerializeField]
    private float collisionResolutionFactor = 0.5f;

    [SerializeField]
    private float sightRange = 1.4f;
    [SerializeField]
    private float actionRange = 0.5f;
    [SerializeField]
    private bool meleeAction = false;

    [SerializeField]
    private bool log = false;

    [SerializeField]
    private SteeringValuesAuthoring steeringValues = null;
    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }
        Layout layout = MapManager.ActiveMap.layout;
        var fractionalHex = layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));




        entityManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = fractionalHex });
        entityManager.AddSharedComponentData<Parent>(entity, new Parent());//not assigned


        entityManager.AddComponentData<OnReinforcement>(entity, new OnReinforcement());

        //health
        entityManager.AddComponentData<Health>(entity, new Health() { MaxHealth = health, CurrentHealth = health });

        //Act
        entityManager.AddComponentData<ActionAttributes>(entity, new ActionAttributes() { ActRange = (Fix64)actionRange, Melee = meleeAction });

        //Pathfinding components.
        entityManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsTorefreshPath, TurnsWithoutRefresh = 0 });
        entityManager.AddComponentData<PathWaypointIndex>(entity, new PathWaypointIndex() { Value = 0 });
        var buffer = entityManager.AddBuffer<PathWaypoint>(entity);

        //Steering
        entityManager.AddComponentData<Steering>(entity, new Steering()
        {
            targetWeight = steeringValues.targetWeight,
            flockWeight = steeringValues.flockWeight,
            previousDirectionWeight = steeringValues.directionWight,

            cohesionWeight = steeringValues.cohesionWeight,
            separationWeight = steeringValues.separationWeight,
            groupalSeparationWeight = steeringValues.groupalSeparationWeigth,
            alineationWeight = steeringValues.alienationWeight,

            satisfactionDistance = (Fix64)steeringValues.satisfactionArea,
            separationDistance = (Fix64)steeringValues.separationDistance,
            singleSeparationDistance = (Fix64)steeringValues.singleSeparationDistance
        }); ;

        //Collision
        entityManager.AddComponentData<Collider>(entity, new Collider()
        {
            Radius = (Fix64)radious,
            CollisionPushIntensity = (Fix64)collisionIntensity,
            CollisionResolutionFactor = (Fix64)collisionResolutionFactor,
            Layer = ColliderLayer.UNIT
        });

        //team (assigned the value in "AssigningUnitParentAuthoring")
        entityManager.AddComponentData<Team>(entity, new Team());

        //target find components
        entityManager.AddComponentData<SightRange>(entity, new SightRange() { Value = (Fix64)sightRange });


        //General Movement Components         
        entityManager.AddComponentData<Speed>(entity, new Speed() { Value = (Fix64)speed });
        entityManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });
        entityManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget());
        entityManager.AddComponentData<DesiredMovement>(entity, new DesiredMovement());

        entityManager.AddComponentData(entity, new WaypointReachedDistance() { Value = (Fix64)waypointReachedDistance });

    }
    private void SetParentRelatedComponents()
    {
        Debug.Assert(parentEntityFilter != null, "You must assign the parent of this entity!!!");
        var parentEntity = parentEntityFilter.Entity;

        var entityManager = World.Active.EntityManager;
        int team = entityManager.GetComponentData<Team>(parentEntity).Number;


        var entityFilter = GetComponent<EntityFilter>();
        Debug.Assert(entityFilter != null, "this GO needs a entity filter to work!");
        entityManager.SetComponentData<Team>(entityFilter.Entity, new Team() { Number = team });
        entityManager.SetSharedComponentData<Parent>(entityFilter.Entity, new Parent() { ParentEntity = parentEntity });
    }
    protected void Start()
    {
        SetParentRelatedComponents();
    }
}
