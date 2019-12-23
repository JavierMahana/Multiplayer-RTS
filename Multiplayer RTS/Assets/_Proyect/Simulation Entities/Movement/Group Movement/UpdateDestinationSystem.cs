using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;


//este systema debe ver, para el destiantion y para el group target destination*: 
// -el problema de la isla
// -si es esta ocupada
[DisableAutoCreation]
//aun no esta creada
public class UpdateDestinationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "this system requires a map to function correctly");

        Entities.ForEach((ref DestinationHex destination, ref MovementState movementState) => 
        {
            if (movementState.DestinationReached)
            {
                return;
            }

            bool destinationIsOccupied = HexIsOccupied(destination.FinalDestination, map.map);
            if (destinationIsOccupied)
            {
                
            }

        });
    }
    private static bool HexIsOccupied(Hex hex, RuntimeMap map)
    {
        if (map.OcupationMapValues.TryGetValue(hex, out bool free))
        {
            return !free;
        }

        else
        {
            return true;
        }
    }
}
