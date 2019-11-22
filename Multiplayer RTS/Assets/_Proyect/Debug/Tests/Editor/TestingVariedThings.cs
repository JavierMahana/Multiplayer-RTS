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
