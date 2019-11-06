using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public static class MapUtilities 
{
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
}
