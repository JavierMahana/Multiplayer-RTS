using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class MovementFinisherSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MovementState movementState, ref HexPosition position, ref DestinationHex destination) => 
        {
            movementState.PreviousStepDestiantionReached = movementState.DestinationReached;


            var distanceToDestination = position.HexCoordinates.Distance((FractionalHex)destination.FinalDestination);            
            if (distanceToDestination <= movementState.DestinationIsReachedDistance)
            {
                movementState.HexOcuppied = destination.FinalDestination;

                movementState.DestinationReached = true;
            }
            else
            {
                movementState.DestinationReached = false;
            }
        });
    }
}