using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using FixMath.NET;
using Sirenix.OdinInspector;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using Photon.Pun;

[DisallowMultipleComponent]
[RequiresEntityConversion]
//starts on reinforcement mode by default
public class UnitAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public AssigningUnitParentAuthoring parentUnitLink;
    public float waypointReachedDistance;
    public float speed;
    public int turnsTorefreshPath;

    public float radious;
    public float collisionIntensity;
    [Range(0, 1)]
    public float collisionResolutionFactor = 1;

    public float sightRange = 1.4f;
    public float actionRange;

    public bool log;

    public SteeringValuesAuthoring steeringValues;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }
        Layout layout = MapManager.ActiveMap.layout;
        var fractionalHex = layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));




        dstManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = fractionalHex });
        dstManager.AddSharedComponentData<Parent>(entity, new Parent() );//not assigned


        dstManager.AddComponentData<OnReinforcement>(entity, new OnReinforcement());


        //Act
        dstManager.AddComponentData<ActionAttributes>(entity, new ActionAttributes() { ActRange = (Fix64)actionRange});

        //Pathfinding
        dstManager.AddComponentData<RefreshPathTimer>(entity, new RefreshPathTimer() { TurnsRequired = turnsTorefreshPath, TurnsWithoutRefresh = 0 });
        dstManager.AddComponentData<PathWaypointIndex>(entity, new PathWaypointIndex() { Value = 0 });
        var buffer = dstManager.AddBuffer<PathWaypoint>(entity);

        //Steering
        dstManager.AddComponentData<Steering>(entity, new Steering()
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
        dstManager.AddComponentData<Collider>(entity, new Collider()
        {
            Radious = (Fix64)radious,
            CollisionPushIntensity = (Fix64)collisionIntensity,
            CollisionResolutionFactor = (Fix64)collisionResolutionFactor,
            Layer = ColliderLayer.UNIT
        });

        //team (assigned the value in "AssigningUnitParentAuthoring")
        dstManager.AddComponentData<Team>(entity, new Team());

        //target find components
        dstManager.AddComponentData<SightRange>(entity, new SightRange() { Value = (Fix64)sightRange });


        //General Movement Components         
        dstManager.AddComponentData<Speed>(entity, new Speed() { Value = (Fix64)speed });        
        dstManager.AddComponentData<DirectionAverage>(entity, new DirectionAverage() { Value = FractionalHex.Zero, PreviousDirection1 = FractionalHex.Zero, PreviousDirection2 = FractionalHex.Zero });
        dstManager.AddComponentData<SteeringTarget>(entity, new SteeringTarget());
        dstManager.AddComponentData<DesiredMovement>(entity, new DesiredMovement());

        dstManager.AddComponentData(entity, new WaypointReachedDistance() { Value = (Fix64)waypointReachedDistance });


        parentUnitLink.UnitEntityCreatedCallback(entity);
    }   
}
