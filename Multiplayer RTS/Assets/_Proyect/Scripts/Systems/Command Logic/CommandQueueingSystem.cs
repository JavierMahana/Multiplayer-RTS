using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateBefore(typeof(CommandExecutionSystem))]
public class CommandQueueingSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ExecuteLockstepTurnLogicFlag execute) =>
        {
            
            CommandStorageSystem.QueueVolatileCommands(
                LockstepTurnFinisherSystem.LockstepTurnCounter + LockstepLockSystem.NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE);
        });
    }
}