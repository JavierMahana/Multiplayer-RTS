using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FixMath.NET;

public class HexConversionTest
{
    // the function don't ereturn the same exact params, because of the impresition it has.
    [Test]
    public void ConversionToHexAndWorldAreInversal()
    {
        //setup
        Layout l = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));

        Vector3 position = new Vector3(0.5f, 6.1f);
        FixVector2 posFix = new FixVector2((Fix64)position.x, (Fix64)position.y);

        FractionalHex hex = l.WorldToFractionalHex(posFix);
        FixVector2 pos2 = l.HexToWorld(hex);
        
        FixVector2 pos3 = l.HexToWorld(l.WorldToFractionalHex(pos2));
        FixVector2 pos4 = l.HexToWorld(l.WorldToFractionalHex(pos3));
        FixVector2 pos5 = l.HexToWorld(l.WorldToFractionalHex(pos4));
        FixVector2 pos6 = l.HexToWorld(l.WorldToFractionalHex(pos5));

        Debug.Log($"pos6: {pos6.x}|{pos6.y}");
        Assert.AreEqual(posFix, pos2);
    }
}
