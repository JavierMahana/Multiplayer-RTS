using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using FixMath.NET;

public class ChangeSteeringValuesInPlayMode : MonoBehaviour
{


    public SteeringValuesAuthoring monitoringValues;
    private EntityQuery m_SteeringEntities;
    //public int targetWeight;
    //public int directionWight;
    //public int flockWeight;

    //public int alienationWeight;
    //public int cohesionWeight;
    //public int separationWeight;
    //public int groupalSeparationWeigth;

    //public float satisfactionArea;
    //public float separationDistance;

    // Start is called before the first frame update
    void Start()
    {
        m_SteeringEntities = World.Active.EntityManager.CreateEntityQuery(typeof(Steering));

        //targetWeight = monitoringValues.targetWeight;
        //directionWight = monitoringValues.directionWight;
        //flockWeight = monitoringValues.flockWeight;

        //alienationWeight = monitoringValues.alienationWeight;
        //cohesionWeight = monitoringValues.cohesionWeight;
        //separationWeight = monitoringValues.separationWeight;
        //groupalSeparationWeigth = monitoringValues.separationWeight;

        //satisfactionArea = monitoringValues.satisfactionArea;
    }

    // Update is called once per frame
    void Update()
    {
        var frameSteering = new Steering() 
        {
            targetWeight = monitoringValues.targetWeight,
            flockWeight = monitoringValues.flockWeight,
            previousDirectionWeight = monitoringValues.directionWight,

            cohesionWeight = monitoringValues.cohesionWeight,
            separationWeight = monitoringValues.separationWeight,
            groupalSeparationWeight = monitoringValues.groupalSeparationWeigth,
            alineationWeight = monitoringValues.alienationWeight,

            satisfactionDistance = (Fix64)monitoringValues.satisfactionArea,
            separationDistance = (Fix64)monitoringValues.separationDistance
        };

        var entities = m_SteeringEntities.ToEntityArray(Allocator.TempJob);
        foreach (var entity in entities)
        {
            World.Active.EntityManager.SetComponentData<Steering>(entity, frameSteering);
        }
        entities.Dispose();
    }
}
