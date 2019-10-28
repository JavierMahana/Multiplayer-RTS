using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using Unity.Mathematics;

public struct Layout 
{
    public Layout(Orientation orientation, FixVector2 size, FixVector2 origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }

    public readonly Orientation orientation;
    public readonly FixVector2 size;
    public readonly FixVector2 origin;


    public Hex PixelToHex(int2 p)
    {
        Orientation M = orientation;
        var point = new FixVector2(((Fix64)p.x + origin.x) / size.x, ((Fix64)p.y + origin.y) / size.y);
        Fix64 q = (M.d11 * point.x + M.d21 * point.y) * size.x;
        Fix64 r = ((M.d12 * point.x) + M.d22 * point.y) * size.y;

        var fractional = new FractionalHex(q, r);
        return fractional.Round();
    }
    public FractionalHex PixelToFractionalHex(int2 p)
    {
        Orientation M = orientation;
        var point = new FixVector2(((Fix64)p.x + origin.x)/size.x , ((Fix64)p.y + origin.y) / size.y);
        Fix64 q = (M.d11 * point.x + M.d21 * point.y) * size.x;
        Fix64 r = ((M.d12 * point.x) + M.d22 * point.y) * size.y;


        return new FractionalHex(q,r);
    }

    public FixVector2 HexToPixel(Hex h)
    {
        Orientation M = orientation;
        Fix64 x = (M.f11 * (Fix64)h.q + M.f12 * (Fix64)h.r) * size.x;
        Fix64 y =  (M.f21 * (Fix64)h.q + M.f22 * (Fix64)h.r) * size.y;

        return new FixVector2(x + origin.x, y + origin.y);
    }

    public FixVector2 CornerOffSet(int corner)
    {
        corner %= 6;
        Orientation M = orientation;
        //funcion y en practica esta bien pero no entiendo la logica. (usa angulos negativos)
        Fix64 angle = (Fix64)2 * Fix64.Pi * (M.angle - (Fix64)corner) / (Fix64)6;
        return new FixVector2(size.x * Fix64.Cos(angle), size.y * Fix64.Sin(angle));
    }
    public FixVector2[] Corners(Hex hex)
    {
        var corners = new FixVector2[6];
        var hexPixelPos = HexToPixel(hex);
        for (int i = 0; i < 6; i++)
        {            
            var offset = CornerOffSet(i);
            corners[i] = hexPixelPos + offset;
        }
        return corners;
    }
}
