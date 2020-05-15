using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FixMath.NET;

public class FunnelAlgorthimTest 
{

    [Test]
    public void HexInBewtweenTest()
    {
        var startPoint_1 = (FractionalHex)new Hex(0,1) + new FractionalHex(-(Fix64)0.25, Fix64.Zero);
        var endPoint_1 = (FractionalHex)new Hex(1,-1) + new FractionalHex(-(Fix64)0.25, Fix64.Zero);
        var hexes = Hex.HexesInBetween(startPoint_1, endPoint_1);

        Assert.IsTrue(hexes.Count == 3);
        Assert.AreEqual(new Hex(0, 1), hexes[0]);
        Assert.AreEqual(new Hex(0,0), hexes[1]);
        Assert.AreEqual(new Hex(1, -1), hexes[2]);
    }
    [Test]
    public void LinePath()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = false
                }
            },         
            {
                new Hex(1,-1),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },          
            {
                new Hex(1,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },            
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            }        
           
        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        var simplifiedPath = PathFindingSystem.HexFunnelAlgorithm
            (
            new List<Hex>()
            {
                new Hex(0,1),
                new Hex(1,0),
                new Hex(1,-1)
            },true, activeMap
            );


        Assert.AreEqual(2, simplifiedPath.Count);
        Assert.AreEqual(new Hex(1, 0), simplifiedPath[0]);
        Assert.AreEqual(new Hex(1, -1), simplifiedPath[1]);
    }

    [Test]
    public void LineVerticalPath()
    {
        var geoMap = new Dictionary<Hex, GeographicTile>
        {
            {
                new Hex(0,0),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-1,1),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(0,1),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-2,2),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-1,2),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-2,3),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-1,3),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },
            {
                new Hex(-2,4),
                new GeographicTile()
                {
                    heightLevel = MapHeight.l0,
                    walkable = true
                }
            },

        };
        RuntimeMap map = new RuntimeMap(geoMap, 100);
        var layout = new Layout(Orientation.pointy, new FixVector2(1, 1), new FixVector2(0, 0));
        ActiveMap activeMap = new ActiveMap(map, layout);

        var simplifiedPath = PathFindingSystem.HexFunnelAlgorithm
            (
            new List<Hex>()
            {
                new Hex(0,0),
                new Hex(-1,1),
                new Hex(-1,2),
                new Hex(-2,3),
                new Hex(-2,4),
            },false , activeMap
            );

        foreach (var item in simplifiedPath)
        {
            Debug.Log(item);
        }
        Assert.AreEqual(2, simplifiedPath.Count);

        var simplifiedPath2 = PathFindingSystem.HexFunnelAlgorithm
    (
    new List<Hex>()
    {
                new Hex(0,0),
                new Hex(-1,1),
                new Hex(-2,2),
                new Hex(-2,3),
                new Hex(-2,4),
    },false, activeMap
    );

        foreach (var item in simplifiedPath2)
        {
            Debug.Log(item);
        }
        Assert.AreEqual(2, simplifiedPath2.Count);
    }
}
