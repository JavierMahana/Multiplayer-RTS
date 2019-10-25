using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using UnityEngine;

[UpdateBefore(typeof(LockstepTurnFinisherSystem))]
public class CommandExecutionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ExecuteLockstepTurnLogicFlag execute) =>
        {
            //Move Commands
            if (CommandStorageSystem.QueuedMoveCommands.TryGetValue(LockstepTurnFinisherSystem.LockstepTurnCounter, out var MoveCommands)) 
            {
                Debug.Log($"Executing a comand in the turn: {LockstepTurnFinisherSystem.LockstepTurnCounter}");
                foreach (MoveCommand command in MoveCommands)
                {
                    if (CommandUtils.CommandIsValid(command, World))
                    {
                        ExecuteCommand(command);
                    }
                }
            }
            //Other Commands
        });
    }



    private void ExecuteCommand(MoveCommand command)    
    {
        EntityManager.SetComponentData(command.Target, command.MoveComponent);
    }
}