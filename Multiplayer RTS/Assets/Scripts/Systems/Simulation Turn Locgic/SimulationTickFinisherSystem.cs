using System;
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
        Entities.WithAll<Simulate, SimulationFinisherState>().ForEach((Entity entity) =>
        {
            if (SimulationAuthoritativeSystem.ExecuteSimulationTick)
            {
                TickCounter++;
                SimulationAuthoritativeSystem.ExecuteSimulationTick = false;
            }
        });





        Entities.WithNone<SimulationFinisherState>().WithAll<Simulate>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<SimulationFinisherState>(entity);
        });

        Entities.WithNone<Simulate>().WithAll<SimulationFinisherState>().ForEach((Entity entity) =>
        {
            ResetState();

            EntityManager.RemoveComponent<SimulationFinisherState>(entity);
        });

    }

    private void ResetState()
    {
        TickCounter = 0;
    }
}