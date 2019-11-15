using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class VolatileCommandSystem : ComponentSystem
{
    //this system manages the volatile commands of each lockstep turn. 
    protected override void OnUpdate()
    {
        SendCommandsToNetwork();
        CommandStorageSystem.QueueVolatileCommands(MainSimulationLoopSystem.CurrentLockstepTurn + MainSimulationLoopSystem.COMMANDS_DELAY);
    }

    private void SendCommandsToNetwork()
    {
        if (OfflineMode.OffLineMode)
            return;

        int turnToExecuteTheCommands = MainSimulationLoopSystem.CurrentLockstepTurn + MainSimulationLoopSystem.COMMANDS_DELAY;
        byte eventCode;
        object[] content;

        Debug.Log($"On CommandNetworkSender, volatile commands to send {CommandStorageSystem.AreVolatileCommands()}");
        if (CommandStorageSystem.AreVolatileCommands())
        {
            eventCode = (byte)NetworkEventTypes.CommandEventCode;
            object[] commands = CommandStorageSystem.GetAllVolatileCommandsSerialized();
            content = new object[]
            {
                    turnToExecuteTheCommands,
                    commands[0]
            };
        }
        else
        {
            eventCode = (byte)NetworkEventTypes.EmptyCommandEventCode;
            content = new object[] { turnToExecuteTheCommands };
        }

        RaiseEventOptions raiseOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions() { Reliability = true };

        PhotonNetwork.RaiseEvent(eventCode, content, raiseOptions, sendOptions);

    }
}