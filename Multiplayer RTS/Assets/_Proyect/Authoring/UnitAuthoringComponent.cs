﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using FixMath.NET;
using Sirenix.OdinInspector;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using Photon.Pun;

[DisallowMultipleComponent]
[RequireComponent(typeof(EntityFilter))]
//starts on reinforcement mode by default
public class UnitAuthoringComponent : EntityAuthoringBase
{


    [SerializeField]
    private EntityFilter parentEntityFilter;


    [SerializeField]
    private float waypointReachedDistance;
    [SerializeField]
    private float speed;
    [SerializeField]
    private int turnsTorefreshPath;

    [SerializeField]
    private float radious;
    [SerializeField]
    private float collisionIntensity;
    [Range(0, 1)]
    [SerializeField]
    private float collisionResolutionFactor = 1;

    [SerializeField]
    private float sightRange = 1.4f;
    [SerializeField]
    private float actionRange;

    [SerializeField]
    private bool log;

    [SerializeField]
    private SteeringValuesAuthoring steeringValues;
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
        entityManager.AddSharedComponentData<Parent>(entity, new Parent() );//not assigned


        entityManager.AddComponentData<OnReinforcement>(entity, new OnReinforcement());


        //Act
        entityManager.AddComponentData<ActionAttributes>(entity, new ActionAttributes() { ActRange = (Fix64)actionRange});

        //Pathfinding
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
            Radious = (Fix64)radious,
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
    private void Start()
    {
        SetParentRelatedComponents();
    }
}
