using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


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
        public void BoxingArrays()
        {
            MoveCommand[] c = new MoveCommand[] { new MoveCommand() {MoveComponent = new MovementTarget() {TargetPostion = new float3(1,1,1) } }, new MoveCommand(), new MoveCommand() };
            object o = c;
            MoveCommand[] moveCommands = (MoveCommand[])o;

            Assert.IsTrue(moveCommands.Length == 3);
            Assert.AreEqual(new float3(1,1,1), moveCommands[0].MoveComponent.TargetPostion);
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
