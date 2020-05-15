using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;
using Unity.Mathematics;

public struct Layout 
{
    public static Layout NotInitialized => new Layout(Orientation.pointy, new FixVector2(0, 0), new FixVector2(0, 0), false);
    public Layout(Orientation orientation, FixVector2 size, FixVector2 origin, bool initialized = true)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
        this.initialized = initialized;
    }

    public readonly bool initialized;
    public readonly Orientation orientation;
    public readonly FixVector2 size;
    public readonly FixVector2 origin;
    /// <summary>
    /// WARNING: non deterministic, don't use in the simulation!
    /// </summary>
    public FractionalHex PixelToFractionaHex(Vector2 mousePos, Camera camera)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        var worldPos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.nearClipPlane));
        var fixWorldPos = new FixVector2((Fix64)worldPos.x, (Fix64)worldPos.y);
        return WorldToFractionalHex(fixWorldPos);
    }
    public Hex PixelToHex(Vector2 mousePos, Camera camera)
    {
        return PixelToFractionaHex(mousePos, camera).Round();
    }

    public Hex WorldToHex(FixVector2 p)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        Orientation M = orientation;
        var point = new FixVector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        Fix64 q = (M.d11 * point.x + M.d12 * point.y) ;
        Fix64 r = ((M.d21 * point.x) + M.d22 * point.y) ;

        var fractional = new FractionalHex(q, r);
        return fractional.Round();
    }
    public FractionalHex WorldToFractionalHex(FixVector2 p)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        Orientation M = orientation;
        var point = new FixVector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        Fix64 q = (M.d11 * point.x + M.d12 * point.y);
        Fix64 r = ((M.d21 * point.x) + M.d22 * point.y);


        return new FractionalHex(q,r);
    }

    public FixVector2 HexToWorld(Hex h)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        Orientation M = orientation;
        Fix64 x = (M.f11 * (Fix64)h.q + M.f12 * (Fix64)h.r) * size.x;
        Fix64 y =  (M.f21 * (Fix64)h.q + M.f22 * (Fix64)h.r) * size.y;

        return new FixVector2(x + origin.x, y + origin.y);
    }
    public FixVector2 HexToWorld(FractionalHex fractionalHex)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        Orientation M = orientation;
        Fix64 x = (M.f11 * (Fix64)fractionalHex.q + M.f12 * (Fix64)fractionalHex.r) * size.x;
        Fix64 y = (M.f21 * (Fix64)fractionalHex.q + M.f22 * (Fix64)fractionalHex.r) * size.y;

        return new FixVector2(x + origin.x, y + origin.y);
    }


    public FixVector2 CornerOffSet(int corner)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        corner %= 6;
        Orientation M = orientation;
        //funcion y en practica esta bien pero no entiendo la logica. (usa angulos negativos)
        Fix64 angle = (Fix64)2 * Fix64.Pi * (M.angle - (Fix64)corner) / (Fix64)6;
        return new FixVector2(size.x * Fix64.Cos(angle), size.y * Fix64.Sin(angle));
    }
    public FixVector2[] Corners(Hex hex)
    {
        if (!initialized) throw new System.Exception("The Layout is not initialized and cannot be used.");
        var corners = new FixVector2[6];
        var hexPixelPos = HexToWorld(hex);
        for (int i = 0; i < 6; i++)
        {            
            var offset = CornerOffSet(i);
            corners[i] = hexPixelPos + offset;
        }
        return corners;
    }

    /// <summary>
    /// NON DETERMINISTIC - use only for visuals.
    /// </summary>
    public Vector2 SingleHexWorldSize => new Vector2(1.73205f * (float)size.x, 2f * (float)size.y);
    /// <summary>
    /// NON DETERMINISTIC - use only for visuals.
    /// It gets the perfect rect given this layout, proportions and padding.
    /// </summary>
    public Rect GetWorldCoordinatesRect(Vector2Int proportions, Vector2 borderPadding)
    {
        var hexWorldSize = SingleHexWorldSize;

        var xMin = (float)origin.x - ((hexWorldSize.x * 0.5f) + borderPadding.x);
        var yMin = (float)origin.y - ((hexWorldSize.y * 0.5f) + borderPadding.y);
        var minPos = new Vector2(xMin, yMin);
        var width = (proportions.x * hexWorldSize.x) + (hexWorldSize.x * 0.5f) + (2 * borderPadding.x);
        var height = ((proportions.y - 1) * hexWorldSize.y * 0.75f) + (hexWorldSize.y + 2 * borderPadding.y);
        var size = new Vector2(width, height);

        return new Rect(minPos, size);
    }
    /// <summary>
    /// NON DETERMINISTIC - use only for visuals.
    /// It gets a rect given this layout, proportions, padding and a aspect ratio.
    /// </summary>
    public Rect GetWorldCoordinatesRect(Vector2Int proportions, Vector2 borderPadding,  Vector2Int aspectRatio)
    {
        int yRatio = aspectRatio.y;
        int xRatio = aspectRatio.x;
        if (yRatio < 0 || xRatio < 0)
            throw new System.ArgumentException($"The aspect ratio is {aspectRatio}. And it must be positive and greater than 0!");

        var rectWithoutAspectRatio = GetWorldCoordinatesRect(proportions, borderPadding);
        var rectSize = rectWithoutAspectRatio.size;
        var rectCenter = rectWithoutAspectRatio.center;
        if(yRatio == xRatio)
        {
            if (rectSize.y < rectSize.x)
            {
                var newHight = rectSize.x;
                var minPos = new Vector2(rectWithoutAspectRatio.xMin, rectCenter.y - newHight * 0.5f);
                return new Rect(minPos, new Vector2(rectSize.x, newHight));
            }
            else 
            {
                var newWidth = rectSize.y;
                var minPos = new Vector2(rectCenter.x - newWidth * 0.5f, rectWithoutAspectRatio.yMin);
                return new Rect(minPos, new Vector2(newWidth, rectSize.y));
            }
        }
        else if (rectSize.y < rectSize.x)
        {
            Debug.Assert(yRatio < xRatio, "The aspect ratio doesn't make any sense! It makes that some parts of the map are not included");

            //X depend on the size of Y
            var newWidth = rectSize.y * xRatio / yRatio;
            var minPos = new Vector2(rectCenter.x - newWidth * 0.5f, rectWithoutAspectRatio.yMin);
            return new Rect(minPos, new Vector2(newWidth, rectSize.y));
        }
        else 
        {
            Debug.Assert(yRatio > xRatio, "The aspect ratio doesn't make any sense! It makes that some parts of the map are not included");

            //Y depend on the size of X
            var newHight = rectSize.x * yRatio / xRatio;
            var minPos = new Vector2(rectWithoutAspectRatio.xMin, rectCenter.y - newHight * 0.5f);
            return new Rect(minPos, new Vector2(rectSize.x, newHight));
        }
    }
}
