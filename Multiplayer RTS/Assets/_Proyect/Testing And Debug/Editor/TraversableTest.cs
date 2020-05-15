using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using FixMath.NET;
using FixMath;
using Unity.Entities;
using Unity.Collections;

public class TraversableTest
{
    //same Hex
    [Test]
    public void SameHexesAreTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                         isSlope = false
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(0, 0), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }
    [Test]
    public void SameHexesOnSlopeAreTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                         isSlope = true,
                        heightSide_0tr = MapHeight.l1,
                        heightSide_1r = MapHeight.l1,
                        heightSide_2dr = MapHeight.l0 | MapHeight.l1,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l0 | MapHeight.l1
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(0, 0), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }

    //movementMap test
    [Test]
    public void  UnitMap_NotWalkableTilesAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);
        activeMap.map.SetUnitOcupationMapValue(new Hex(0,0), false);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.UNIT, activeMap));

    }

    //unitMap Test
    [Test]
    public void MovementMap_NotWalkableTilesAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);
        activeMap.map.SetMovementMapValue(new Hex(0, 0), false);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.MOVEMENT, activeMap));

    }

    //open/closedTile
    [Test]
    public void NotWalkableTilesAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = false,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));

    }
    //normal tile en normal tile
    [Test]
    public void DiferentHeightTilesAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));

    }
    [Test]
    public void SameHeightTilesAreTaversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,

                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));

    }
    //slope en tile
    [Test]
    public void SlopeEdgeAndTileOfTheSameHeightAreTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l1,
                        heightSide_1r = MapHeight.l1,
                        heightSide_2dr = MapHeight.l0 | MapHeight.l1,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l0 | MapHeight.l1
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(0, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));

    }
    [Test]
    public void SlopeEdgeAndTileOfDiferentHeightAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l1,
                        heightSide_1r = MapHeight.l1,
                        heightSide_2dr = MapHeight.l0 | MapHeight.l1,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l0 | MapHeight.l1
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(0, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }
    [Test]
    public void TheSlopeEdgeOfASlopeAndATileAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = false,
                    }
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l0,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l1,
                        heightSide_1r = MapHeight.l1,
                        heightSide_2dr = MapHeight.l0 | MapHeight.l1,
                        heightSide_3dl = MapHeight.l0,
                        heightSide_4l = MapHeight.l0,
                        heightSide_5tl = MapHeight.l0 | MapHeight.l1
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);


        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(-1, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }

    //slope en slope
    [Test]
    public void TwoEdgesOfSlopesOfTheSameHeightAreTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l3,
                        heightSide_2dr = MapHeight.l3 | MapHeight.l4,
                        heightSide_3dl = MapHeight.l4,
                        heightSide_4l = MapHeight.l3 | MapHeight.l4,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            },
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l4,
                        heightSide_1r = MapHeight.l3 | MapHeight.l4,
                        heightSide_2dr = MapHeight.l3,
                        heightSide_3dl = MapHeight.l3,
                        heightSide_4l = MapHeight.l3,
                        heightSide_5tl = MapHeight.l3 | MapHeight.l4
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(0, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }
    [Test]
    public void TwoEdgesOfSlopesOfDiferentHeightsAreNotTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l3,
                        heightSide_2dr = MapHeight.l3 | MapHeight.l4,
                        heightSide_3dl = MapHeight.l4,
                        heightSide_4l = MapHeight.l3 | MapHeight.l4,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            },
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l3,
                        heightSide_2dr = MapHeight.l3 | MapHeight.l4,
                        heightSide_3dl = MapHeight.l4,
                        heightSide_4l = MapHeight.l3 | MapHeight.l4,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        Assert.IsFalse(MapUtilities.IsTraversable(new Hex(0, 1), new Hex(0, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }
    [Test]
    public void TheSlopeEdgeOfASlopeAndOtherSlopeEdgeOfTheSameHeightAreTraversables()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l3 | MapHeight.l4,
                        heightSide_2dr = MapHeight.l4,
                        heightSide_3dl = MapHeight.l4,
                        heightSide_4l = MapHeight.l3 | MapHeight.l4,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            },
            {
                new Hex(1,0),
                new GeographicTile()
                {
                    walkable = true,
                    heightLevel = MapHeight.l3 | MapHeight.l4,
                    slopeData = new SlopeData()
                    {
                        isSlope = true,
                        heightSide_0tr = MapHeight.l3,
                        heightSide_1r = MapHeight.l3 | MapHeight.l4,
                        heightSide_2dr = MapHeight.l4,
                        heightSide_3dl = MapHeight.l4,
                        heightSide_4l = MapHeight.l3 | MapHeight.l4,
                        heightSide_5tl = MapHeight.l3
                    }
                }
            }
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        Assert.IsTrue(MapUtilities.IsTraversable(new Hex(0, 0), new Hex(1, 0), MapUtilities.MapType.GEOGRAPHYC, activeMap));
    }
}
