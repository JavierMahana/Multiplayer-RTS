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

namespace Javier.Testing
{
    [Flags]
    public enum FlagTestEnum : byte
    {
        NONE = 0,
        ONE = 1,
        TWO = 2,
        THREE = 4,
        FOUR = 8,
        FIVE = 16,
        SIX = 32,
        SEVEN = 64,
        EIGHT = 128
    }




    public interface Itest { }
    public struct TestStruct
    {
        public Itest testField;
    }
    public struct A : Itest
    {
        public float f;
    }
    public struct B : Itest
    {
        public int i;
    }
    public class TestingVariedThings
    {
        [Test]
        public void ElevationFactorLevelTestNoSlopes()
        {
            MapHeight l0 = MapHeight.l0;
            MapHeight l1 = MapHeight.l1;
            MapHeight l01 = l0 & l1;

            GeographicTile tile0 = new GeographicTile() { heightLevel = l0, slopeData = new SlopeData() { isSlope = false } };
            GeographicTile tile1 = new GeographicTile() { heightLevel = l1, slopeData = new SlopeData() { isSlope = false } };
            GeographicTile tile01 = new GeographicTile() { heightLevel = l01, slopeData = new SlopeData() { isSlope = false } };

            Assert.AreEqual(0, PositionListener.GetElevationFactorLevel(tile0, FractionalHex.Zero));
            Assert.AreEqual(1, PositionListener.GetElevationFactorLevel(tile1, FractionalHex.Zero));
            Assert.AreEqual(0, PositionListener.GetElevationFactorLevel(tile01, FractionalHex.Zero));

        }

        [Test]
        public void NormilizeDepthWorksAsIntended()
        {
            float x = 9999;
            float y = -9999;

            x = PositionListener.NormalizeDepth(x);
            y = PositionListener.NormalizeDepth(y);

            Assert.That(y < 1, "value y is more than 1");
            Assert.That(y > 0, "value y is less than 0");
            Assert.That(x < 1, "value x is more than 1");
            Assert.That(x > 0, "value x is less than 0");
            Assert.That(x > y, "the equallity don't work");

        }

        [Test]
        public void CompatibleHeightLevelWorkAsIntended()
        {
            MapHeight l0_1 = MapHeight.l0 | MapHeight.l1;
            MapHeight l0 = MapHeight.l0;
            MapHeight l1 = MapHeight.l1;

            Assert.IsTrue(MapUtilities.CompatibleHeightLevel(l0_1, l0));
            Assert.IsTrue(MapUtilities.CompatibleHeightLevel(l0_1, l1));

            Assert.IsFalse(MapUtilities.CompatibleHeightLevel(l1, l0));
        }
        [Test]
        public void MapHeightsAreComparable()
        {
            MapHeight l0_1 = MapHeight.l0 | MapHeight.l1;
            MapHeight l0 = MapHeight.l0;
            MapHeight l1 = MapHeight.l1;
            MapHeight l2 = MapHeight.l2;

            Assert.IsTrue(l0_1 < l2);
            Assert.IsTrue(l1 < l2);
            Assert.IsTrue(l0 < l2);
        }

        [Test]
        public void TheHexesInRangeReturnsTheCorrectAmmountOfValuesWithHex()
        {
            Hex hex = new Hex(7,7);
            int range = 6;
            var results = Hex.AllHexesInRange(hex, range);

            int expected = 0;
            for (int i = 0; i <= range; i++)
            {
                expected += i * 6;
            }


            Assert.AreEqual(expected, results.Count);
        }

        [Test]
        public void TheHexesInRangeReturnsTheCorrectAmmountOfValuesWithCenter()
        {
            int range = 18;
            var results = Hex.AllHexesInRange(range, true);

            int expected = 0;
            for (int i = 0; i <= range; i++)
            {
                expected += i * 6;
            }
               

            Assert.AreEqual(expected + 1, results.Count);
        }

        [Test]
        public void ListWithoutAddedItemsHaveLenghtOf0()
        {
            var list = new List<int>();
            Assert.That(list.Count == 0);
        }
        [Test]
        public void TheHexesInRangeReturnsTheCorrectAmmountOfValues()
        {
            int range = 15;
            var results = Hex.AllHexesInRange(range);

            int expected = 0;
            for (int i = 0; i <= range; i++)
            {
                expected += i * 6;
            }


            Assert.AreEqual(expected, results.Count);
        }
        [Test]
        public void TheHexesInRangeDebugValues()
        {
            int range = 2;
            var results = Hex.AllHexesInRange(range);

            int expected = 0;
            for (int i = 0; i <= range; i++)
            {
                expected += i * 6;
            }

            foreach (var value in results)
            {
                Debug.Log(value);
            }

            Assert.AreEqual(expected, results.Count);
        }

        [Test]
        public void FlagEnumsWorkLikeIThougnTheyWould()
        {
            FlagTestEnum flag = FlagTestEnum.ONE | FlagTestEnum.TWO | FlagTestEnum.THREE;

            Assert.IsTrue(flag.HasFlag(FlagTestEnum.ONE));
            Assert.IsTrue(flag.HasFlag(FlagTestEnum.TWO));
            Assert.IsTrue(flag.HasFlag(FlagTestEnum.THREE));

            Assert.IsFalse(flag.HasFlag(FlagTestEnum.FOUR));
            Assert.IsFalse(flag.HasFlag(FlagTestEnum.FIVE));
            Assert.IsFalse(flag.HasFlag(FlagTestEnum.SIX));
            Assert.IsFalse(flag.HasFlag(FlagTestEnum.SEVEN));
            Assert.IsFalse(flag.HasFlag(FlagTestEnum.EIGHT));

            Assert.AreEqual((byte)7 ,(byte)flag);
        }
        [Test]
        public void FractionalHexFunctionAngleTest()
        {
            var a = FractionalHex.Zero;
            var b = new FractionalHex((Fix64)0.5, Fix64.Zero);

            var direction = new FractionalHex((Fix64)0.5, (Fix64)(-0.5)).Normalized();
            var angle = FractionalHex.Angle(a, direction, b);

            Assert.AreEqual(((Fix64)60/(Fix64)180 * Fix64.Pi) , angle);
        }

        [Test]
        public void TestingClosestPointInLineProduct()
        {
            var direction = new FractionalHex((Fix64)1,(Fix64)(-1)).Normalized();
            var directionManhathan = direction.NormalizedManhathan();

            var start = new Hex(0, 0);
            var goal = new FractionalHex((Fix64).5, (Fix64) (- .5));

            var distanceUWU = FractionalHex.ClosestPointInLine((FractionalHex)start, direction, (FractionalHex)goal);            
            Assert.AreEqual(0.5f , (float)distanceUWU);
        }

        [Test]
        public void UsingTypeFunctionsWorksFineWithInterfaces()
        {
            var testStruct = new TestStruct()
            {
                testField = new A() { f = 5f }
            };


            Assert.AreEqual(typeof(A), testStruct.testField.GetType());


            testStruct.testField = new B() { i = 10 };
            Assert.AreEqual(typeof(B), testStruct.testField.GetType());
        }
    }
}
