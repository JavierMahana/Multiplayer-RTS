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
    public static int GetValueOfHeight(MapHeight heightLevel)
    {
        switch (heightLevel)
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
                Debug.LogWarning("this is not a simple height value(more than 1 flag used). Returning 0");
                return 0;
        }
    }
}
