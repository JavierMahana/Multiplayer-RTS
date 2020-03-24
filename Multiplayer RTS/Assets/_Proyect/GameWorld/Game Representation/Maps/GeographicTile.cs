using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum MapHeight : byte
{    
    l0 = 1 << 0,
    l1 = 1 << 1,
    l2 = 1 << 2,
    l3 = 1 << 3,
    l4 = 1 << 4,
    l5 = 1 << 5,
    l6 = 1 << 6,
    l7 = 1 << 7

}
public struct GeographicTile
{
    public bool walkable;
    public MapHeight heightLevel;

    public bool IsSlope => slopeData.isSlope;
    public SlopeData slopeData;

}
