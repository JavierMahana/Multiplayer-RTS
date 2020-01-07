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
            if(loggExecutionTurn)Debug.Log($"Executing a move comand in the turn: {MainSimulationLoopSystem.CurrentLockstepTurn}");
            foreach (MoveCommand command in MoveCommands)
            {
                if (CommandUtils.CommandIsValid(command, World))
                {
                    ExecuteCommand(command);
                }
            }
        }

        if (CommandStorageSystem.QueuedChangeBehaviourCommands.TryGetValue(MainSimulationLoopSystem.CurrentLockstepTurn, out var changeBehaviourCommands))
        {
            if (loggExecutionTurn) Debug.Log($"Executing a change behaviour comand in the turn: {MainSimulationLoopSystem.CurrentLockstepTurn}");
            foreach (ChangeBehaviourCommand command in changeBehaviourCommands)
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
        Debug.Log("setting the destination by command");
        PostUpdateCommands.SetComponent(command.Target, command.Destination);
        PostUpdateCommands.AddComponent(command.Target, new RefreshPathNow());
    }
    private void ExecuteCommand(ChangeBehaviourCommand command)
    {
        //Debug.Log("setting the destination by command");
        PostUpdateCommands.SetComponent(command.Target, command.NewBehaviour);
        //PostUpdateCommands.AddComponent(command.Target, new RefreshPathNow());
    }
}