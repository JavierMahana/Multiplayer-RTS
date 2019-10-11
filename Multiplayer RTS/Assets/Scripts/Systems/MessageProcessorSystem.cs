using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public struct ProcessedMessage 
{
    public MessageType Type;
    //CommandsToExecute
}
[UpdateAfter(typeof(LockstepSystem))]
public class MessageProcessorSystem : ComponentSystem
{
    private bool logg = true;

    public static bool AllMessagesOfTurnAreRecieved(int turnToCheck)
    {
        if (turnToCheck < LockstepSystem.NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE)
            return true;

        bool commandSelfMessage = false;
        bool commandOtherMessage = false;
        bool confirmMessage = false;

        NativeMultiHashMapIterator<int> iterator;
        if (messagesCache.TryGetFirstValue(turnToCheck, out ProcessedMessage currMessage, out iterator))
        {
            do
            {                
                switch (currMessage.Type)
                {
                    case MessageType.COMMAND_SELF:
                        commandSelfMessage = true;
                        break;
                    case MessageType.COMMAND_OTHER:
                        commandOtherMessage = true;
                        break;
                    case MessageType.CONFIRMATION:
                        confirmMessage = true;
                        break;
                    default:
                        Debug.LogError("invalid message");
                        break;
                }
            } while (messagesCache.TryGetNextValue(out currMessage, ref iterator));
        }
        Debug.Log($"the result of the prossesing of turn:{turnToCheck} is : {commandSelfMessage && confirmMessage && commandOtherMessage}");
        return commandSelfMessage && confirmMessage && commandOtherMessage;
            
    }


    private static NativeMultiHashMap<int, ProcessedMessage> messagesCache;


    protected override void OnUpdate()
    {
        if(logg) Debug.Log($"MessageSystem Running on: {TimeSystem.TotalSimulationTime}");
        Entities.ForEach((Entity entity, ref Message message) =>
        {
            //sending confirmation event to command messages of other clients
            if (message.Type == MessageType.COMMAND_OTHER)
            {
                object[] content = new object[] { message.TurnToExecute };
                byte eventCode = NetworkEventCatchingSystem.ReceivedCommandConfirmationEventCode;
                RaiseEventOptions raiseOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
                SendOptions sendOptions = new SendOptions() {Reliability = true };

                PhotonNetwork.RaiseEvent(eventCode, content, raiseOptions, sendOptions);
            }


            //message added to the cache
            messagesCache.Capacity = messagesCache.Capacity + 1;
            messagesCache.Add(message.TurnToExecute, new ProcessedMessage() { Type = message.Type });

            //message destroyed
            EntityManager.DestroyEntity(entity);
        });
    }

    protected override void OnCreate()
    {
        messagesCache = new NativeMultiHashMap<int, ProcessedMessage>(0, Allocator.Persistent);
    }
    protected override void OnDestroy()
    {
        messagesCache.Clear();
        messagesCache.Dispose();
    }
}
