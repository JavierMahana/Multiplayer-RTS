using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

//this class listens to the entity of the entity filter and moves itself to the corresponding place.
//it diferentiates tiles and other agents.

[RequireComponent(typeof(EntityFilter))]

public class PositionListener : MonoBehaviour
{
    public const float AGENTS_EXTRA_Z_VALUE = 0.5f;
    private EntityFilter entityFilter; 
    private int minZValue;

    private void Awake()
    {
        var camera = Camera.main;
        minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + 1);
        entityFilter = GetComponent<EntityFilter>();
    }
    private void Start()
    {
        var entity = entityFilter.Entity;
        var entityManager = entityFilter.EntityManager;
        Debug.Assert(entityManager.HasComponent<HexPosition>(entity), "the position listener component requires that the entity have a hexposition comp");
    }

    private void Update()
    {
        var activeMap = MapManager.ActiveMap;
        if (MapManager.ActiveMap == null)
            return;

        var hexPosition = entityFilter.EntityManager.GetComponentData<HexPosition>(entityFilter.Entity);
        Fix64 radius = Fix64.Zero;
        if (entityFilter.EntityManager.HasComponent<Collider>(entityFilter.Entity))
        {
            radius = entityFilter.EntityManager.GetComponentData<Collider>(entityFilter.Entity).Radius;
        }

        
        float elevation = MapUtilities.GetElevationOfPosition(hexPosition.HexCoordinates);
        var worldPosition = activeMap.layout.HexToWorld(hexPosition.HexCoordinates);

        //this takes into consideration the radius of the object. so the visuals are not cutted when walking into another hex. the point goes down by the radius.
        var coordinateForZValue = hexPosition.HexCoordinates - new FractionalHex(-(Fix64)0.5, (Fix64)1,-(Fix64)0.5) * radius;
        // if the y coordinate is lower, it must be closer
        float zCoordinate = minZValue + coordinateForZValue.Round().r - AGENTS_EXTRA_Z_VALUE;


        transform.localPosition = new Vector3((float)worldPosition.x, (float)worldPosition.y + elevation, zCoordinate);

    }


    //not used
    public static float NormalizeDepth(float value)
    {
        float maxValue = 10000;
        float minValue = -10000;
        if (value >= maxValue)
        {
            Debug.LogWarning("the input is hiegher or equal that the max, returning max posible value");
            return 0.9999999f;
        }
        else if (value <= minValue)
        {
            Debug.LogWarning("the input is lower or equal that the min, returning min posible value");
            return 0.0000001f;
        }
        else
        {
            //truncate to the 3rd decimal.
            value *= 1000;
            value = Mathf.Round(value);
            value *= 0.001f;

            //add 9.999 and divide by 2
            value += 9999;
            value *= 0.5f;
            value *= 0.0001f;

            if (value >= 1)
                return 0.9999999f;
            else if (value <= 0)
                return 0.0000001f;
            else
                return value;
        }
    }

}
