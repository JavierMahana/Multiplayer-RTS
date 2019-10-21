using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using FixMath.NET;


[UpdateAfter(typeof(LockstepTurnFinisherSystem))]
public class TimeSystem : ComponentSystem
{
    public static readonly Fix64 SimulationDeltaTime = (Fix64)0.05M;
    public static float TotalSimulationTime = 0;
    public static float TimeNotProcessed = 0;

    private void ResetState()
    {
        TotalSimulationTime = 0;
        TimeNotProcessed = 0;
    }

    protected override void OnUpdate()
    {
        Entities.WithAll<Simulate, SimulationTimeSystemState>().ForEach((Entity entity) =>
        {
            if (LockstepLockSystem.LockstepActivated)
            {
                return;
            }
            TotalSimulationTime += Time.deltaTime;
            TimeNotProcessed += Time.deltaTime;
        });






        Entities.WithNone<SimulationTimeSystemState>().WithAll<Simulate>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<SimulationTimeSystemState>(entity);
        });

        Entities.WithNone<Simulate>().WithAll<SimulationTimeSystemState>().ForEach((Entity entity) =>
        {
            ResetState();
            
            EntityManager.RemoveComponent<SimulationTimeSystemState>(entity);
        });



    }
}