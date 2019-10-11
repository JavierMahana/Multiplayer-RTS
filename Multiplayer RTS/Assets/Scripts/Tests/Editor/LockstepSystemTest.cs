using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

//#region test

//[DisableAutoCreation]
//public class SysWithStaticNativeArray : ComponentSystem
//{
//    public static NativeArray<NormalData> staticDataArray;
//    protected override void OnCreate()
//    {
//        staticDataArray = new NativeArray<NormalData>(1, Allocator.Persistent);
//        staticDataArray[0] = new NormalData() { normalInt = 10 }; 
//    }
//    protected override void OnDestroy()
//    {
//        staticDataArray.Dispose();
//    }
//    protected override void OnUpdate() { }

//}
//[DisableAutoCreation]
//public class Sys1 : ComponentSystem
//{
//    public bool ChangeNativeArrayValueIfValueIsEqual(int value)
//    {
//        bool areEqual = value == SysWithStaticNativeArray.staticDataArray[0].normalInt;
//        if (areEqual)
//        {
//            SysWithStaticNativeArray.staticDataArray[0] = new NormalData() { normalInt = value + 1 };
//        }
//        return areEqual;
//    }
//    protected override void OnUpdate() { }
//}
//[DisableAutoCreation]
//public class Sys2 : ComponentSystem
//{
//    protected override void OnUpdate() { }
//}

//public struct SingletonData : IComponentData
//{
//    public int singletonInt;
//}

//public struct NormalData : IComponentData
//{
//    public int normalInt;
//}
//public struct SharedData : ISharedComponentData
//{
//    public int sharedInt;
//}
//#endregion


//#region Test Methods
//public bool TheLockstepPasses(int turn)
//{
//    int currentLockstepTurn = turn;
//    if (currentLockstepTurn < NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE)
//    {
//        return true;
//    }
//    else if (IsPossibleToPassThroughLockstep(currentLockstepTurn))
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}
//public bool TheLockstepPasses(int turn, out int numberOfMessagesCycledTrough)
//{
//    int currentLockstepTurn = turn;
//    if (currentLockstepTurn < NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE)
//    {
//        numberOfMessagesCycledTrough = 0;
//        return true;
//    }
//    else if (IsPossibleToPassThroughLockstep(currentLockstepTurn, out numberOfMessagesCycledTrough))
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}
//private bool IsPossibleToPassThroughLockstep(int currentLockstepTurn, out int numberOfMessagesToCycle)
//{
//    ////m_EntityQuery.ResetFilter();
//    //m_EntityQuery.SetFilter(new TurnToExecute() { turnToExecute = currentLockstepTurn });

//    //var currentTurnMessages = m_EntityQuery.ToComponentDataArray<Message>(Allocator.TempJob);

//    //numberOfMessagesToCycle = currentTurnMessages.Length;

//    //bool syncMessageRecieved = false;
//    //bool confirmationMessageRecieved = false;

//    //for (int i = 0; i < currentTurnMessages.Length; i++)
//    //{
//    //    var message = currentTurnMessages[i];
//    //    if (message.Type == MessageType.SYNC)
//    //        syncMessageRecieved = true;
//    //    if (message.Type == MessageType.CONFIRMATION)
//    //        confirmationMessageRecieved = true;
//    //}

//    //currentTurnMessages.Dispose();

//    //return syncMessageRecieved && confirmationMessageRecieved;
//    numberOfMessagesToCycle = 0;
//    return true;


//}
//#endregion




//public class LockstepSystemTest
//{
//    [Test]
//    public void LockstepCanPassTroughTurns0And1WithoutMessages()
//    {
//        World w = new World("test");
//        var s = w.GetOrCreateSystem<LockstepSystem>();

//        Assert.IsTrue(s.TheLockstepPasses(0));
//        Assert.IsTrue(s.TheLockstepPasses(1));

//        w.DestroySystem(s);
//    }
//    [Test]
//    public void When_ThereAreASyncAndAConfirmationMessageInATurn_Then_LockstepCanPassTroughThatTurn()
//    {
//        World w = new World("test");
//        var s = w.GetOrCreateSystem<LockstepSystem>();

//        int turn = 2;

//        Entity sync = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(sync, new Message() { Type = MessageType.SYNC });
//        w.EntityManager.AddSharedComponentData(sync, new TurnToExecute() { turnToExecute = turn });

//        Entity conf = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(conf, new Message() { Type = MessageType.CONFIRMATION });
//        w.EntityManager.AddSharedComponentData(conf, new TurnToExecute() { turnToExecute = turn });

//        Assert.IsTrue(s.TheLockstepPasses(turn));

//        w.DestroySystem(s);

//    }
//    [Test]
//    public void LockstepOnlyCycleThroughMessagesOfTheTurnThatIsChecking()
//    {
//        World w = new World("test");
//        var s = w.GetOrCreateSystem<LockstepSystem>();

//        int turn = 2;

//        Entity sync = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(sync, new Message() { Type = MessageType.SYNC });
//        w.EntityManager.AddSharedComponentData(sync, new TurnToExecute() { turnToExecute = turn });

//        Entity conf = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(conf, new Message() { Type = MessageType.CONFIRMATION });
//        w.EntityManager.AddSharedComponentData(conf, new TurnToExecute() { turnToExecute = turn });



//        Entity e = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(e, new Message() { Type = MessageType.SYNC });
//        w.EntityManager.AddSharedComponentData(e, new TurnToExecute() { turnToExecute = turn + 1 });

//        s.TheLockstepPasses(turn + 1, out int numberOfCycles);

//        Assert.AreEqual(1, numberOfCycles);

//        w.DestroySystem(s);
//    }
//    [Test]
//    public void LockstepUpdateTheQueryThroughTurns()
//    {
//        World w = new World("test");
//        var s = w.GetOrCreateSystem<LockstepSystem>();

//        int turn = 2;

//        Entity sync = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(sync, new Message() { Type = MessageType.SYNC });
//        w.EntityManager.AddSharedComponentData(sync, new TurnToExecute() { turnToExecute = turn });

//        Entity conf = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(conf, new Message() { Type = MessageType.CONFIRMATION });
//        w.EntityManager.AddSharedComponentData(conf, new TurnToExecute() { turnToExecute = turn });


//        Assert.IsTrue(s.TheLockstepPasses(turn, out int numberOfCycles));
//        Assert.AreEqual(2, numberOfCycles);


//        Entity e = w.EntityManager.CreateEntity();
//        w.EntityManager.AddComponentData(e, new Message() { Type = MessageType.SYNC });
//        w.EntityManager.AddSharedComponentData(e, new TurnToExecute() { turnToExecute = turn + 1 });

//        Assert.IsFalse(s.TheLockstepPasses(turn + 1, out int cyclesP2));
//        Assert.AreEqual(1, cyclesP2);

//        w.DestroySystem(s);
//    }
//}
