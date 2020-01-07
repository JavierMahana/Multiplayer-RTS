using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMap 
{
    

    public RuntimeMap(Map map)
    {
        GeographicMapValues = new Dictionary<Hex, bool>(map.HexWalkableFlags);
        MovementMapValues = new Dictionary<Hex, bool>(GeographicMapValues);
        UnitsMapValues = new Dictionary<Hex, bool>(GeographicMapValues);
    }
    public RuntimeMap(Dictionary<Hex,bool> geographicMap)
    {
        GeographicMapValues = new Dictionary<Hex, bool>(geographicMap);
        MovementMapValues = new Dictionary<Hex, bool>(GeographicMapValues);
        UnitsMapValues = new Dictionary<Hex, bool>(GeographicMapValues);
    }


    public readonly Dictionary<Hex, bool> GeographicMapValues;
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
