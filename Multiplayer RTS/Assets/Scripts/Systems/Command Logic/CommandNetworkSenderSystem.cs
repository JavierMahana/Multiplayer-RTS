using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;

[UpdateBefore(typeof(CommandQueueingSystem))]
public class CommandNetworkSenderSystem : ComponentSystem
{
   
    
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ExecuteLockstepTurnLogicFlag execute) =>
        {
            if (OfflineMode.OffLineMode)
                return;

            int turnToExecute = LockstepTurnFinisherSystem.LockstepTurnCounter + LockstepLockSystem.NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE;
            byte eventCode;
            object[] content;

            Debug.Log($"On CommandNetworkSender, volatile commands to send {CommandStorageSystem.AreVolatileCommands()}");
            if (CommandStorageSystem.AreVolatileCommands())
            {                
                eventCode = (byte)NetworkEventTypes.CommandEventCode;
                object[] commands = CommandStorageSystem.GetAllVolatileCommandsSerialized();
                content = new object[]
                {
                    turnToExecute,
                    commands[0]
                };
            }
            else 
            {
                eventCode = (byte)NetworkEventTypes.EmptyCommandEventCode;
                content = new object[] { turnToExecute };
            }

            RaiseEventOptions raiseOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions() { Reliability = true };

            PhotonNetwork.RaiseEvent(eventCode, content, raiseOptions, sendOptions);

        });
    }
}