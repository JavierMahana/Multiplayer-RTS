using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FixMath.NET;

public class ColisionDistanceTest 
{

    [Test]
    public void RightSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0,0), new FractionalHex((Fix64)0.65,(Fix64)0.2, -(Fix64)0.85));
        Assert.AreEqual(0.25f, (float)a);
    }
    [Test]
    public void LeftSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0, 0), new FractionalHex(-(Fix64)0.65, -(Fix64)0.2, (Fix64)0.85));
        Assert.AreEqual(0.25f, (float)a);
    }
    [Test]
    public void TopRightSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0, 0), new FractionalHex((Fix64)0.2, (Fix64)0.65, -(Fix64)0.85));
        Assert.AreEqual(0.25f, (float)a);
    }
    [Test]
    public void DownLeftSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0, 0), new FractionalHex(-(Fix64)0.2, -(Fix64)0.65, (Fix64)0.85));
        Assert.AreEqual(0.25f, (float)a);
    }
    [Test]
    public void DownRightSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0, 0), new FractionalHex((Fix64)0.65, -(Fix64)0.85, (Fix64)0.2));
        Assert.AreEqual(0.25f, (float)a);
    }
    [Test]
    public void TopLeftSideCollision()
    {
        var a = CollisionSystem.GetCollisionDistance(new Hex(0, 0), new FractionalHex(-(Fix64)0.65, (Fix64)0.85, -(Fix64)0.2));
        Assert.AreEqual(0.25f, (float)a);
    }
}
