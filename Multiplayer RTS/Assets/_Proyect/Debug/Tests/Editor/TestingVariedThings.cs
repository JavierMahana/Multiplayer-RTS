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
        public void TestingClosestPointInLineProduct()
        {
            var direction = new FractionalHex((Fix64)1,(Fix64)(-1)).Normalized();
            var directionManhathan = direction.NormailezedManhathan();

            var start = new Hex(0, 0);
            var goal = new Hex(2, -2);

            var distanceUWU = FractionalHex.ColsestPointInLine((FractionalHex)start, direction, (FractionalHex)goal);
            var dot = FractionalHex.DotProduct(direction, (FractionalHex)(goal - start));
            Assert.AreEqual(2 , (int)distanceUWU);
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
