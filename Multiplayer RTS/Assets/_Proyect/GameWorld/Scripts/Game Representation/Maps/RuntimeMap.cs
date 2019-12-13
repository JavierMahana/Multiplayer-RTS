using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMap 
{
    public RuntimeMap(Map map)
    {
        StaticMapValues = new Dictionary<Hex, bool>(map.HexWalkableFlags);
        DinamicMapValues = new Dictionary<Hex, bool>(StaticMapValues);
    }

    public readonly Dictionary<Hex, bool> StaticMapValues;
    public Dictionary<Hex, bool> DinamicMapValues;


    public static Hex FindClosestOpenHex(FractionalHex position, ActiveMap activeMap)
    {
        var closestOpenHex = Hex.Zero;
        Fix64 closestOpenHexDistance = Fix64.MaxValue;
        foreach (var hexValuePair in activeMap.map.DinamicMapValues)
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
