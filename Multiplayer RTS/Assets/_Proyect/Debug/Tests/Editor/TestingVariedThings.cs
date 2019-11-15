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
    public class Syst : ComponentSystem
    {
        public int a = 0;
        public int b = 0;
        protected override void OnUpdate()
        {
            
            Entities.ForEach((ref MovementTarget t) => 
            {
                a++;
                if (a == 1) return;
                b++;
            });
        }
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
        public void ReturnKeywordInsideAForechFuncitionOnlyReturnsForTheSpeciedElement() 
        {
            var world = new World("vvvv");
            var syst = world.GetOrCreateSystem<Syst>();

            NativeArray<Entity> entities = new NativeArray<Entity>(10, Allocator.Temp);
            var archetype = world.EntityManager.CreateArchetype(typeof(MovementTarget));
            world.EntityManager.CreateEntity(archetype, entities);

            syst.Update();

            Assert.AreEqual(syst.a, 10);
            Assert.AreEqual(syst.b, 9);

            entities.Dispose();
            world.Dispose();
        }
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
