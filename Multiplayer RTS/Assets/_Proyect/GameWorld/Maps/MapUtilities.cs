using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public static class MapUtilities 
{    
    public static bool PathToPointIsClear(FractionalHex position, FractionalHex point)
    {
        bool clearPath = true;
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "The Active Map is null!!!");
        var hexesInBewtween = Hex.HexesInBetween(position, point);
        foreach (Hex hex in hexesInBewtween)
        {
            if (map.map.MovementMapValues.TryGetValue(hex, out bool walkable))
            {
                if (!walkable)
                {
                    clearPath = false;
                    break;
                }
            }
            else
            {
                clearPath = false;
                break;
            }
        }

        return clearPath;
    }

    public static Layout GetHexLayout(Vector2 tileSize, Mesh mesh, Vector2 originPoint)
    {
        Vector3 quadExtents = mesh.bounds.extents;
        FixVector2 origin = new FixVector2((Fix64)originPoint.x, (Fix64)originPoint.y);
        FixVector2 hexSize = new FixVector2(
            ((Fix64)tileSize.x * (Fix64)quadExtents.x),
            ((Fix64)tileSize.y * (Fix64)quadExtents.y));
        Layout hexLayout = new Layout(Orientation.pointy, hexSize, origin);
        return hexLayout;
    }


    public static Hex FindClosestOpenHex(FractionalHex position, RuntimeMap map)
    {
        var closestOpenHex = Hex.Zero;
        Fix64 closestOpenHexDistance = Fix64.MaxValue;
        foreach (var hexValuePair in map.MovementMapValues)
        {
            if (!hexValuePair.Value) { continue; }

            var hex = hexValuePair.Key;
            var distance = position.Distance((FractionalHex)hex);

            if (distance <= closestOpenHexDistance)
            {
                closestOpenHex = hex;
                closestOpenHexDistance = distance;
            }
        }

        return closestOpenHex;
    }


}
