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

}
