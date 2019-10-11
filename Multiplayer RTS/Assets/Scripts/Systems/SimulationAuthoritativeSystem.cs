using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(TimeSystem))]
public class SimulationAuthoritativeSystem : ComponentSystem
{
    public static bool ExecuteSimulationTick = false;
    protected override void OnUpdate()
    {
        if (LockstepSystem.LockstepActivated)
        {
            return;
        }

        Fix64 notProcessedTime = (Fix64)TimeSystem.TimeNotProcessed;
        if (TimeSystem.SimulationDeltaTime <= notProcessedTime)
        {
            Fix64 difference = notProcessedTime - TimeSystem.SimulationDeltaTime;
            if (difference >= TimeSystem.SimulationDeltaTime)
            {
                Debug.LogWarning("Your frame rate is dangerously low!");
            }

            TimeSystem.TimeNotProcessed = (float)difference;
            ExecuteSimulationTick = true;
        }

    }
}