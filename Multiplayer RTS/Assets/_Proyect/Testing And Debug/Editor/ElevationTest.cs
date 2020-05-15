using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FixMath.NET;

public class ElevationTest 
{
    [Test]
    public void TestElevatioFunction_OnSimple2_R()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l4,
                        heightSide_2dr = MapHeight.l3,
                        heightSide_3dl = MapHeight.l3,
                        heightSide_4l = MapHeight.l3,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        var elevation1 = MapUtilities.GetElevationOfPosition(new FractionalHex(Fix64.Zero, Fix64.Zero), activeMap);
        var elevation2 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.25f, Fix64.Zero, -(Fix64)0.25f), activeMap);
        var elevation3 = MapUtilities.GetElevationOfPosition(new FractionalHex(Fix64.Zero, (Fix64)0.5f, -(Fix64)0.5f), activeMap);
        var elevation4 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.5f, Fix64.Zero, -(Fix64)0.5), activeMap);
        var elevation5 = MapUtilities.GetElevationOfPosition(new FractionalHex(-(Fix64)0.5f, Fix64.Zero, (Fix64)0.5), activeMap);
        var elevation6 = MapUtilities.GetElevationOfPosition(new FractionalHex(-(Fix64)0.25f, Fix64.Zero, (Fix64)0.25f), activeMap);

        Assert.AreEqual(300,elevation1);
        Assert.AreEqual(350,elevation2);
        Assert.AreEqual(350, elevation3);
        Assert.AreEqual(400, elevation4);
        Assert.AreEqual(300, elevation5);
        Assert.AreEqual(300, elevation6);
    }
    [Test]
    public void TestElevatioFunction_OnSimple_R()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0 | MapHeight.l1,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l0,
                        heightSide_1r = MapHeight.l1,
                        heightSide_2dr = MapHeight.l0,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l0
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        var elevation1 = MapUtilities.GetElevationOfPosition(new FractionalHex(Fix64.Zero, Fix64.Zero), activeMap);
        var elevation2 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.25f, Fix64.Zero, -(Fix64)0.25f), activeMap);
        var elevation3 = MapUtilities.GetElevationOfPosition(new FractionalHex(Fix64.Zero, (Fix64)0.5f, -(Fix64)0.5f), activeMap);
        var elevation4 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.5f, Fix64.Zero, -(Fix64)0.5), activeMap);
        var elevation5 = MapUtilities.GetElevationOfPosition(new FractionalHex(-(Fix64)0.5f, Fix64.Zero, (Fix64)0.5), activeMap);
        var elevation6 = MapUtilities.GetElevationOfPosition(new FractionalHex(-(Fix64)0.25f, Fix64.Zero, (Fix64)0.25f), activeMap);

        Assert.AreEqual(0, elevation1);
        Assert.AreEqual(50, elevation2);
        Assert.AreEqual(50, elevation3);
        Assert.AreEqual(100, elevation4);
        Assert.AreEqual(0, elevation5);
        Assert.AreEqual(0, elevation6);
    }
    [Test]
    public void TestElevatioFunction_OnDouble_T()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l1,
                        heightSide_1r = MapHeight.l0,
                        heightSide_2dr = MapHeight.l0,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l1
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        var elevation1 = MapUtilities.GetElevationOfPosition(new FractionalHex(Fix64.Zero, Fix64.Zero), activeMap);
        var elevation2 = MapUtilities.GetElevationOfPosition(new FractionalHex(-(Fix64)0.2886835f, (Fix64)0.577367f, -(Fix64)0.2886835f), activeMap);
        var elevation3 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.2886835f, -(Fix64)0.577367f, (Fix64)0.2886835f), activeMap);
        var elevation4 = MapUtilities.GetElevationOfPosition(new FractionalHex((Fix64)0.2886835f, (Fix64)0.2886835f, -(Fix64)0.577367f), activeMap);
        //var elevation5 = activeMap.GetElevationOfPosition(new FractionalHex(-(Fix64)0.5f, Fix64.Zero, (Fix64)0.5));
        //var elevation6 = activeMap.GetElevationOfPosition(new FractionalHex(-(Fix64)0.25f, Fix64.Zero, (Fix64)0.25f));

        Assert.AreEqual(50, elevation1);
        Assert.AreEqual(100, elevation2);
        Assert.AreEqual(0, elevation3);
        Assert.AreEqual(100, elevation4);
        //Assert.AreEqual(0, elevation5);
        //Assert.AreEqual(0, elevation6);
    }
}
