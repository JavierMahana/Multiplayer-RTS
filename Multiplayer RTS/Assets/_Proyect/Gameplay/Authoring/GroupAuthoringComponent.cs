using FixMath.NET;
using Javier.RTS;
using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Photon.Pun;


[DisallowMultipleComponent]
public class GroupAuthoringComponent : BaseGroupAuthoringComponent
{

}

/*
 public const int AGENTS_Z_VALUE = 9;
    public const int TILE_Z_VALUE = 10;

    private bool initialized;
    private int minZValue;

    /// <summary>
    /// This function prossesses the tile and the postion to return the multiplication
    /// factor that must be multiplied with the map's "ElevationPerHeightLevel".
    /// It must considerate if the tile is a slope, and if is one it must calculate the 
    /// multiplication factor based in the position and in what edges the slope is.
    /// </summary>
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
            value = math.round(value);
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

    protected override void OnUpdate()
    {
        if (MapManager.ActiveMap == null)
            return ;
        else
        {
            if (!initialized)
            {
                var camera = Camera.main;
                minZValue = Mathf.CeilToInt(camera.transform.position.z + camera.nearClipPlane + 1);
                initialized = true;
            }

            Entities.WithAll<HexTile>().ForEach((ref Translation translation, ref HexPosition hexPosition) =>
            {
                var worldPos = MapManager.ActiveMap.layout.HexToWorld(hexPosition.HexCoordinates);
                int zCoord = minZValue + hexPosition.HexCoordinates.Round().r;
                translation.Value = new float3((float)worldPos.x, (float)worldPos.y, zCoord);
            });

            Entities.WithNone<HexTile>().ForEach((ref Translation translation, ref HexPosition hexPosition) =>
            {
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
                var worldPos = activeMap.layout.HexToWorld(hexPosition.HexCoordinates);

                // if the y coordinate is lower, it must be closer
                float zCoord = minZValue + hexPosition.HexCoordinates.Round().r - (1 - NormalizeDepth((float)worldPos.y));

                translation.Value = new float3((float)worldPos.x, (float)worldPos.y + extraElevation, zCoord);
            });
        }
    }*/
