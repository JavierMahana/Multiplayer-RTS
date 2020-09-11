using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;


public enum PosListenerMode
{
    HEX_POS = 0,
    BUILDING = 1,
    RESOURCE = 2,
    SUBSTITUTE = 3
}

/// <summary>
///this class listens to the entity of the entity filter and moves itself to the corresponding place.
///TO WORK NEEDS: a HexPosition or a Building or a ResourceSource component.
///it diferentiates tiles and other agents.
/// </summary>
[RequireComponent(typeof(EntityFilter))]

public class PositionListener : MonoBehaviour
{
    public const float AGENTS_EXTRA_Z_VALUE = 0.5f;
    private EntityFilter entityFilter;
    private PosListenerMode mode;
    private int minZValue;
    private const int DISTANCE_TO_CAMERA = 4;

    private void Awake()
    {
        var camera = Camera.main;
        minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + DISTANCE_TO_CAMERA);
        entityFilter = GetComponent<EntityFilter>();
    }
    private void Start()
    {
        var camera = Camera.main;
        minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + DISTANCE_TO_CAMERA);
        entityFilter = GetComponent<EntityFilter>();


        var entity = entityFilter.Entity;
        var entityManager = entityFilter.EntityManager;
        bool hasHexPos = entityManager.HasComponent<HexPosition>(entity);
        bool hasBuilding = entityManager.HasComponent<Building>(entity);
        bool hasResSource = entityManager.HasComponent<ResourceSource>(entity);
        bool hasSubstitute = entityManager.HasComponent<Substitute>(entity);
        Debug.Assert(hasHexPos || hasBuilding || hasResSource || hasSubstitute, "the position listener component requires that the entity have a hexposition or a building or a resourceSource comp");
        if (hasHexPos)
            mode = PosListenerMode.HEX_POS;
        else if (hasBuilding)
            mode = PosListenerMode.BUILDING;
        else if (hasResSource)
            mode = PosListenerMode.RESOURCE;
        else if (hasSubstitute)
            mode = PosListenerMode.SUBSTITUTE;
        else
        {
            Debug.LogWarning("The pos listener will not work because the entity don't have any of the required comps to work.");
            mode = (PosListenerMode)666;
        }
    }

    private void Update()
    {
        var activeMap = MapManager.ActiveMap;
        if (MapManager.ActiveMap == null)
            return;

        FractionalHex hexCoords;

        switch (mode)
        {
            case PosListenerMode.HEX_POS:
                if (entityFilter.EntityManager.HasComponent<HexPosition>(entityFilter.Entity))
                    hexCoords = entityFilter.EntityManager.GetComponentData<HexPosition>(entityFilter.Entity).HexCoordinates;
                else
                    return;
                break;
            case PosListenerMode.BUILDING:                
                if (entityFilter.EntityManager.HasComponent<Building>(entityFilter.Entity))
                    hexCoords = (FractionalHex)entityFilter.EntityManager.GetComponentData<Building>(entityFilter.Entity).position;
                else                 
                    return;                                                 
                break;
            case PosListenerMode.RESOURCE:               
                if (entityFilter.EntityManager.HasComponent<ResourceSource>(entityFilter.Entity))
                    hexCoords = (FractionalHex)entityFilter.EntityManager.GetComponentData<ResourceSource>(entityFilter.Entity).position;
                else
                    return;
                break;
            case PosListenerMode.SUBSTITUTE:
                if (entityFilter.EntityManager.HasComponent<Substitute>(entityFilter.Entity))
                    hexCoords = (FractionalHex)entityFilter.EntityManager.GetComponentData<Substitute>(entityFilter.Entity).position;
                else
                    return;
                break;
            default:
                return;
        }
         
        Fix64 radius = Fix64.Zero;
        if (entityFilter.EntityManager.HasComponent<Collider>(entityFilter.Entity))
        {
            radius = entityFilter.EntityManager.GetComponentData<Collider>(entityFilter.Entity).Radius;
        }

        
        float elevation = MapUtilities.GetElevationOfPosition(hexCoords);
        var worldPosition = activeMap.layout.HexToWorld(hexCoords);

        //this takes into consideration the radius of the object. so the visuals are not cutted when walking into another hex. the point goes down by the radius.
        var coordinateForZValue = hexCoords - new FractionalHex(-(Fix64)0.5, (Fix64)1,-(Fix64)0.5) * radius;
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

