using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class UpdateOcupationMapSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.ForEach((ref MovementState movementState) => 
        {
            var map = MapManager.ActiveMap;
            Debug.Assert(map != null, "this system cannot work without a map");

            bool newSpotOccupied = movementState.DestinationReached && !movementState.PreviousStepDestiantionReached;
            if (newSpotOccupied)
            {
                map.map.SetOcupationMapValue(movementState.HexOcuppied, false);
            }

            bool spotDesocupied = !movementState.DestinationReached && movementState.PreviousStepDestiantionReached;
            if (spotDesocupied)
            {
                map.map.SetOcupationMapValue(movementState.HexOcuppied, true);
            }
        });
    }
}