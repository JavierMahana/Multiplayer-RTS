using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(EntityFilter))]
public class PositionListener : MonoBehaviour
{
    private enum TypeOfPositionTransformation 
    {
        AGENT,
        MAP_TILE
    }
    //public const int AGENTS_Z_VALUE = 9;
    //public const int TILE_Z_VALUE = 10;
    private EntityFilter entityFilter;
    private int minZValue;    
    private TypeOfPositionTransformation typeOfEntity;


    private void Awake()
    {
        var camera = Camera.main;
        minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + 1);
        entityFilter = GetComponent<EntityFilter>();
    }
    private void Start()
    {
        //set typeOfEntity member
        var entity = entityFilter.Entity;
        var entityManager = entityFilter.EntityManager;
        Debug.Assert(entityManager.HasComponent<HexPosition>(entity), "the position listener component requires that the entity have a hexposition comp");
        if (entityManager.HasComponent<HexTile>(entity))
        {
            typeOfEntity = TypeOfPositionTransformation.MAP_TILE;
        }
        else
        {
            typeOfEntity = TypeOfPositionTransformation.AGENT;
        }
        
    }

    private void Update()
    {
        if (MapManager.ActiveMap == null)
            return;

        var hexPosition = entityFilter.EntityManager.GetComponentData<HexPosition>(entityFilter.Entity);

        switch (typeOfEntity)
        {
            case TypeOfPositionTransformation.AGENT:
                Hex currentHex = hexPosition.HexCoordinates.Round();
                var activeMap = MapManager.ActiveMap;

                float extraElevation;
                if (activeMap.map.GeographicMapValues.TryGetValue(currentHex, out GeographicTile geoTile))
                {
                    extraElevation = GetElevationFactorLevel(geoTile, hexPosition.HexCoordinates) * activeMap.map.ElevationPerHeightLevel;
                }
                else
                {
                    Debug.LogWarning("entity is not inside the map");
                    extraElevation = 0;
                }
                var worldPosition = activeMap.layout.HexToWorld(hexPosition.HexCoordinates);

                // if the y coordinate is lower, it must be closer
                float zCoordinate = minZValue + hexPosition.HexCoordinates.Round().r - (1 - NormalizeDepth((float)worldPosition.y));

                transform.localPosition = new Vector3((float)worldPosition.x, (float)worldPosition.y + extraElevation, zCoordinate);
                break;


            case TypeOfPositionTransformation.MAP_TILE:
                var worldPos = MapManager.ActiveMap.layout.HexToWorld(hexPosition.HexCoordinates);
                int zCoord = minZValue + hexPosition.HexCoordinates.Round().r;
                transform.localPosition = new Vector3((float)worldPos.x, (float)worldPos.y, zCoord);
                break;


            default:
                throw new System.NotImplementedException();
        }
    }

    public static float GetElevationFactorLevel(GeographicTile geoTile, FractionalHex position)
    {
        if (geoTile.IsSlope)
        {
            Debug.LogWarning("the elevation factor is not implemented in slopes yet");
            return 0;
        }
        else
        {
            switch (geoTile.heightLevel)
            {
                case MapHeight.l0:
                    return 0;
                case MapHeight.l1:
                    return 1;
                case MapHeight.l2:
                    return 2;
                case MapHeight.l3:
                    return 3;
                case MapHeight.l4:
                    return 4;
                case MapHeight.l5:
                    return 5;
                case MapHeight.l6:
                    return 6;
                case MapHeight.l7:
                    return 7;
                default:
                    Debug.LogWarning("this is not a valid case for not slope tiles");
                    return 0;
            }
        }
    }
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
