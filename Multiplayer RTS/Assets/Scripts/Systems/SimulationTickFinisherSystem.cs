using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(SimulationAuthoritativeSystem))]
public class SimulationTickFinisherSystem : ComponentSystem
{
    public static int TickCounter = 0;

    protected override void OnUpdate()
    {
        if (SimulationAuthoritativeSystem.ExecuteSimulationTick)
        {
            TickCounter++;
            SimulationAuthoritativeSystem.ExecuteSimulationTick = false;
        }
    }
}