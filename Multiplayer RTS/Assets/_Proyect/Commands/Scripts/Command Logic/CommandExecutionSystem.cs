using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using UnityEngine;

[DisableAutoCreation]
public class CommandExecutionSystem : ComponentSystem
{
    private bool loggExecutionTurn = false;
    protected override void OnUpdate()
    {
        //Move Commands
        if (CommandStorageSystem.QueuedMoveCommands.TryGetValue(MainSimulationLoopSystem.CurrentLockstepTurn, out var MoveCommands))
        {
            if(loggExecutionTurn)Debug.Log($"Executing a comand in the turn: {MainSimulationLoopSystem.CurrentLockstepTurn}");
            foreach (MoveCommand command in MoveCommands)
            {
                if (CommandUtils.CommandIsValid(command, World))
                {
                    ExecuteCommand(command);
                }
            }
        }
        //Other Commands

    }



    private void ExecuteCommand(MoveCommand command)    
    {
        PostUpdateCommands.SetComponent(command.Target, command.Destination);
        PostUpdateCommands.AddComponent(command.Target, new RefreshPathNow());
    }
}