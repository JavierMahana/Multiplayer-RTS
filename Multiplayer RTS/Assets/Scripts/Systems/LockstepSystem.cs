using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class LockstepSystem : ComponentSystem
{
    #region Public Static Fields (todas estas variables despues se podrian transformar en propiedades para tener más seguridad)

    public static bool LockstepActivated = false;
    public static int LockstepTurnCounter = 0;
    public const int NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE = 2;
    #endregion
    private bool logg = true;
    private int lastGameTickWithOpenLocktep = int.MinValue;
    private const int TICKS_REQUIRED_FOR_LOCKSTEP_TURN = 4;


    #region Component System CallBacks
    protected override void OnUpdate()
    {
        int currTick = SimulationTickFinisherSystem.TickCounter;
        if (currTick % TICKS_REQUIRED_FOR_LOCKSTEP_TURN == 0 && currTick != lastGameTickWithOpenLocktep)
        {


            if (TestMode.Instance.OffLineModeTest)
            {
                PassThrougLockstepOffline();
                lastGameTickWithOpenLocktep = currTick;
            }
            else if(MessageProcessorSystem.AllMessagesOfTurnAreRecieved(LockstepTurnCounter))
            {
                PassThroughLockstepOnline();
            }
        }
    }
    #endregion
    #region Private Methods  
    private void PassThroughLockstepOnline()
    {
        SendEmptyCommandToNetwork();
        PassThrougLockstepOffline();
    }
    private void PassThrougLockstepOffline()
    {
        if (logg) Debug.Log($"Passing through lockstep turn {LockstepTurnCounter}| executed on: time:{TimeSystem.TotalSimulationTime} game tick:{SimulationTickFinisherSystem.TickCounter}");

        //create self message;
        var entity = EntityManager.CreateEntity(typeof(Message));
        EntityManager.SetComponentData(entity, new Message() 
        { 
            TurnToExecute = LockstepTurnCounter + NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE,
            Type = MessageType.COMMAND_SELF 
        });


        lastGameTickWithOpenLocktep = LockstepTurnCounter;
        LockstepActivated = true;
        LockstepTurnCounter += 1;
        
    }
    private void SendEmptyCommandToNetwork()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("in room");
            object[] content = new object[] { LockstepTurnCounter + NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE };
            byte eventCode = NetworkEventCatchingSystem.EmptyCommandEventCode;
            RaiseEventOptions raiseOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions() { Reliability = true };

            PhotonNetwork.RaiseEvent(eventCode, content, raiseOptions, sendOptions);
            Debug.Log("Empty Command event sended!");
        }
        else
        {
            Debug.Log("Join a room to send network events");
        }
    }
    private bool IsPossibleToPassThroughLockstep(int currentLockstepTurn)
    {
        //m_EntityQuery.ResetFilter();
        //m_EntityQuery.SetFilter(new TurnToExecute() { turnToExecute = currentLockstepTurn });

        //var currentTurnMessages = m_EntityQuery.ToComponentDataArray<Message>(Allocator.TempJob);

        //bool syncMessageRecieved = false;
        //bool confirmationMessageRecieved = false;

        //for (int i = 0; i < currentTurnMessages.Length; i++)
        //{
        //    var message = currentTurnMessages[i];
        //    if (message.Type == MessageType.SYNC)
        //        syncMessageRecieved = true;
        //    if (message.Type == MessageType.CONFIRMATION)
        //        confirmationMessageRecieved = true;
        //}

        //currentTurnMessages.Dispose();

        //return syncMessageRecieved && confirmationMessageRecieved;
        return false;
    }    

    private void SendRPCToNetwork(int currentLockstepTurn)
    {
        int turnToExecuteTheMessage = currentLockstepTurn + NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE;
        NetworkMessageManager.Instance.CallRPC(turnToExecuteTheMessage, MessageType.COMMAND_SELF);
    }
    #endregion

}

