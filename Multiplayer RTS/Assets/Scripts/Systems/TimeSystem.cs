using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using FixMath.NET;

[UpdateAfter(typeof(LockstepSystem))]
public class TimeSystem : ComponentSystem
{
    public static readonly Fix64 SimulationDeltaTime = (Fix64)0.05M;
    public static float TotalSimulationTime = 0;
    public static float TimeNotProcessed = 0;

    protected override void OnUpdate()
    {              
        if (LockstepSystem.LockstepActivated)
        {
            return;
        }
        TotalSimulationTime += Time.deltaTime;
        TimeNotProcessed += Time.deltaTime;
    }
}