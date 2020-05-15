using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using System;


public enum NetworkEventTypes : byte
{
    EmptyCommandEventCode = 1,
    ReceivedCommandConfirmationEventCode = 2,
    CommandEventCode = 3,
    TestEvent = 199
}

//Este sistema es creado automaticamente y lo que hace es subscribirse a eventos y dar las respuestas necesarias.
//esto debe ser cambiado por el Command storage system envolais
public class NetworkEventProcessorSystem : ComponentSystem, IOnEventCallback
{
    private bool logg = false;

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == (byte)NetworkEventTypes.EmptyCommandEventCode) { EmptyCommandCallback(photonEvent); }
        else if (photonEvent.Code == (byte)NetworkEventTypes.ReceivedCommandConfirmationEventCode) { ReceivedCommandConfirmationCallback(photonEvent); }
        else if (photonEvent.Code == (byte)NetworkEventTypes.CommandEventCode) { CommandCallback(photonEvent); }
        else if (photonEvent.Code == (byte)NetworkEventTypes.TestEvent) { TestEventRecieved(photonEvent); }
    }



    protected override void OnCreate()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    protected override void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    protected override void OnUpdate()
    {

    }

    #region private methods
    private void CreateConfirmationLockstepCheck(int turn)
    {
        var entity = EntityManager.CreateEntity(typeof(LockstepCkeck));
        EntityManager.SetComponentData(entity, new LockstepCkeck() { Turn = turn, Type = LockstepCheckType.CONFIRMATION });
    }
    private void CreateCommandLockstepCheck(int turn)
    {
        var entity = EntityManager.CreateEntity(typeof(LockstepCkeck));
        EntityManager.SetComponentData(entity, new LockstepCkeck() { Turn = turn, Type = LockstepCheckType.COMMAND });
    }
    private void SendCommandConfirmationEvent(int turn)
    {
        object[] content = new object[] { turn };
        byte eventCode = (byte)NetworkEventTypes.ReceivedCommandConfirmationEventCode;
        RaiseEventOptions raiseOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions() { Reliability = true };

        PhotonNetwork.RaiseEvent(eventCode, content, raiseOptions, sendOptions);
    }
    #endregion
    #region Network EventCallBacks
    //event data layout:
    //it must be a object array.
    //and always the first element is the turn of execution.
    private void CommandCallback(EventData photonEvent)
    {
        //null check is important
       
        object[] eventData = (object[])photonEvent.CustomData;
        int turnToExecute = (int)eventData[0];

        
        object[] serializedMoveCommands = (object[])eventData[1];
        MoveCommand[] moveCommands;
        if (serializedMoveCommands != null)
        {
            moveCommands = new MoveCommand[serializedMoveCommands.Length];
            for (int i = 0; i < serializedMoveCommands.Length; i++)
            {
                moveCommands[i] = CommandUtils.DeserializeMoveCommand((object[])serializedMoveCommands[i]);
            }
        }
        else
            moveCommands = null;
        
        

        object[] serializedChangeBehaviourCommand = (object[])eventData[2];
        ChangeBehaviourCommand[] changeBehaviourCommands;
        if (serializedChangeBehaviourCommand != null)
        {
            changeBehaviourCommands = new ChangeBehaviourCommand[serializedChangeBehaviourCommand.Length];
            for (int i = 0; i < changeBehaviourCommands.Length; i++)
            {
                changeBehaviourCommands[i] = CommandUtils.DeserializeChangeBehaviourCommand((object[])serializedChangeBehaviourCommand[i]);
            }
        }
        else
            changeBehaviourCommands = null;
        


        //otros commandos
        CommandStorageSystem.QueueNetworkedCommands(turnToExecute, moveCommands, changeBehaviourCommands);

        CreateCommandLockstepCheck(turnToExecute);
        SendCommandConfirmationEvent(turnToExecute);
    }

    private void EmptyCommandCallback(EventData photonEvent)
    {
        object[] eventData = (object[])photonEvent.CustomData;
        int turnToExecute = (int)eventData[0];

        CreateCommandLockstepCheck(turnToExecute);
        SendCommandConfirmationEvent(turnToExecute);

        if(logg)Debug.Log($"EmptyCommand recieved For Turn {turnToExecute}");
    }
    private void ReceivedCommandConfirmationCallback(EventData photonEvent)
    {
        object[] eventData = (object[])photonEvent.CustomData;
        int turnToExecute = (int)eventData[0];

        CreateConfirmationLockstepCheck(turnToExecute);

        if (logg) Debug.Log($"Recieved command Confirmation recieved For Turn {turnToExecute}");
    }
    private void TestEventRecieved(EventData photonEvent)
    {
        byte id = (byte)photonEvent.CustomData;
        if (logg) Debug.Log($"test event recieved in system. ID: {id}");
    }
    #endregion

}