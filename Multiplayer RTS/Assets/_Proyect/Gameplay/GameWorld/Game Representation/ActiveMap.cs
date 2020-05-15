using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public class ActiveMap
{
    public RuntimeMap map = null;
    public Layout layout = Layout.NotInitialized;
    public float heightPerElevationUnit = 0;

    public ActiveMap(RuntimeMap map, Layout layout)
    {
        this.map = map;
        this.layout = layout;
        heightPerElevationUnit = map.ElevationPerHeightLevel;
    }


}
