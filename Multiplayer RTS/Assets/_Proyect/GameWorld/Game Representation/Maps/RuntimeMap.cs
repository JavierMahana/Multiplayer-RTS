using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//debo hacer que la slope data se valla a la info geografica.
public class RuntimeMap 
{    
    public RuntimeMap(Map map)
    {
        ElevationPerHeightLevel = map.elevationPerHeightLevel;

        GeographicMapValues = new Dictionary<Hex, GeographicTile>();
        foreach (var mapKeyValue in map.HexWalkableFlags)
        {
            Hex hex       = mapKeyValue.Key;
            bool walkable = mapKeyValue.Value;
            var height    = map.HexHeights[hex];
            var slopeData = map.HexSlopeDatas[hex];
            var geographicTile = new GeographicTile() { walkable = walkable, heightLevel = height, slopeData = slopeData };

            GeographicMapValues.Add(hex, geographicTile);
        }
        MovementMapValues   = new Dictionary<Hex, bool>(map.HexWalkableFlags);
        UnitsMapValues      = new Dictionary<Hex, bool>(map.HexWalkableFlags);
    }
    public RuntimeMap(Dictionary<Hex,GeographicTile> geographicMap, float elevationPerHeightLevel)
    {
        ElevationPerHeightLevel = elevationPerHeightLevel;

        GeographicMapValues = new Dictionary<Hex, GeographicTile>(geographicMap);


        MovementMapValues = new Dictionary<Hex, bool>();
        UnitsMapValues = new Dictionary<Hex, bool>();
        foreach (var geoMapKeyValue in geographicMap)
        {
            Hex hex = geoMapKeyValue.Key;
            bool walkable = geoMapKeyValue.Value.walkable;

            MovementMapValues.Add(hex, walkable);
            UnitsMapValues.Add(hex, walkable);
        }

    }
    public readonly float ElevationPerHeightLevel;

    public readonly Dictionary<Hex, GeographicTile> GeographicMapValues;
    /// <summary>
    /// true is walkable.
    /// </summary>
    public Dictionary<Hex, bool> MovementMapValues { get; private set; }
    /// <summary>
    /// true is free.
    /// </summary>
    public Dictionary<Hex, bool> UnitsMapValues { get; private set; }

    public void SetMovementMapValue(Hex key, bool newValue)
    {
        if (! MovementMapValues.ContainsKey(key))
        {
            throw new System.ArgumentException("You are trying to set a key that doesn't exist");            
        }

        MovementMapValues[key] = newValue;
        UnitsMapValues[key] = newValue;
    }
    public void SetOcupationMapValue(Hex key, bool newValue)
    {
        if (!UnitsMapValues.ContainsKey(key))
        {
            throw new System.ArgumentException("You are trying to set a key that doesn't exist");
        }

        UnitsMapValues[key] = newValue;
    }

}
