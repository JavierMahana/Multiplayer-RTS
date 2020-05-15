using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


public class LockstepSystemTest
{
    [Test]
    public void Isolated_When_TheTwoLockstepChecksOfATurnAreCreated_Then_TheLockstepCheckOfThatTurnWillPass()
    {
        //SETUP
        World world = new World("Test World");
        //DefaultWorldInitialization.Initialize("Test World", editorWorld: false);

        var checkSystem = world.GetOrCreateSystem<LockstepCheckSystem>();

        Entity e1 = world.EntityManager.CreateEntity(typeof(LockstepCkeck));
        Entity e2 = world.EntityManager.CreateEntity(typeof(LockstepCkeck));

        world.EntityManager.SetComponentData(e1, new LockstepCkeck() { Turn = 4, Type = LockstepCheckType.COMMAND });
        world.EntityManager.SetComponentData(e2, new LockstepCkeck() { Turn = 4, Type = LockstepCheckType.CONFIRMATION });

        //ACTION    
        checkSystem.Update();


        //ASSERT
        Assert.IsTrue(LockstepCheckSystem.AllCheksOfTurnAreRecieved(4));



        World.DisposeAllWorlds();
    }
    [Test]
    public void Isolated_When_TheLockstepCheckPasses_Then_TheLocksOfThatTurnAreDeleted()
    {
        //SETUP
        World world = new World("Test World");
        //DefaultWorldInitialization.Initialize("Test World", editorWorld: false);

        var checkSystem = world.GetOrCreateSystem<LockstepCheckSystem>();

        Entity e1 = world.EntityManager.CreateEntity(typeof(LockstepCkeck));
        Entity e2 = world.EntityManager.CreateEntity(typeof(LockstepCkeck));

        world.EntityManager.SetComponentData(e1, new LockstepCkeck() { Turn = 4, Type = LockstepCheckType.COMMAND });
        world.EntityManager.SetComponentData(e2, new LockstepCkeck() { Turn = 4, Type = LockstepCheckType.CONFIRMATION });

        //ACTION    
        checkSystem.Update();
        LockstepCheckSystem.AllCheksOfTurnAreRecieved(4);

        //ASSERT
        Assert.IsFalse(LockstepCheckSystem.AllCheksOfTurnAreRecieved(4));





        World.DisposeAllWorlds();
    }
    [Test]
    public void Isolated_When_OffLineMode_Then_TheLocksetCheckAllwaysPasses()
    {
        //ACTION    
        OfflineMode.SetOffLineMode(true);

        //ASSERT
        Assert.IsTrue(LockstepCheckSystem.AllCheksOfTurnAreRecieved(4));
        Assert.IsTrue(LockstepCheckSystem.AllCheksOfTurnAreRecieved(499));
        Assert.IsTrue(LockstepCheckSystem.AllCheksOfTurnAreRecieved(41));
        Assert.IsTrue(LockstepCheckSystem.AllCheksOfTurnAreRecieved(0));


        World.DisposeAllWorlds();
    }

}
