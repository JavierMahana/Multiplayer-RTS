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
public class UpdateDestinationSystem : ComponentSystem
{    
    //the path refresh system depends in this.
    protected override void OnUpdate()
    {        
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "this system requires a map to function correctly");

        //priorityTarget variant
        Entities.WithAll<RefreshPathNow>().ForEach((Entity entity, ref HexPosition hexPosition, ref PriorityGroupTarget priorityTarget, ref MovementState movementState) =>
        {
            if (NewDestinationNeeded(hexPosition.HexCoordinates, priorityTarget.TargetHex, map.map, out Hex newDestination))
            {
                priorityTarget.TargetHex = newDestination;
            }
        });


        Entities.WithNone<RefreshPathNow>().ForEach((Entity entity, ref HexPosition hexPosition, ref PriorityGroupTarget priorityTarget, ref MovementState movementState) =>
        {
            if (movementState.DestinationReached)
            {
                return;
            }

            if (NewDestinationNeeded(hexPosition.HexCoordinates, priorityTarget.TargetHex, map.map, out Hex newDestination))
            {
                priorityTarget.TargetHex = newDestination;
            }
        });





        //normal variant (da lo mismo si se actualiza el destino en el entretiempo)
        Entities.WithAll<RefreshPathNow>().ForEach((Entity entity, ref HexPosition hexPosition, ref DestinationHex destination, ref MovementState movementState) =>
        {            
            if (NewDestinationNeeded(hexPosition.HexCoordinates, destination.FinalDestination, map.map, out Hex newDestination))
            {
                destination.FinalDestination = newDestination;
            } 
        });


        Entities.WithNone<RefreshPathNow>().ForEach((Entity entity, ref HexPosition hexPosition, ref DestinationHex destination, ref MovementState movementState) => 
        {
            if (movementState.DestinationReached)
            {
                return;
            }

            if (NewDestinationNeeded(hexPosition.HexCoordinates, destination.FinalDestination, map.map, out Hex newDestination))
            {
                destination.FinalDestination = newDestination;
            }                                   
        });
    }
    /// <summary>
    /// A new destination is needed if you cannot reach the destination or if its occupied
    /// </summary>
    private static bool NewDestinationNeeded(FractionalHex startPos, Hex end, RuntimeMap map, out Hex newDestination)
    {
        Hex startHex = startPos.Round();
        newDestination = end;
        bool destinationIsAvalible = HexIsFreeAndReachable(startHex, end, map);

        if (!destinationIsAvalible)
        {
            //starting in the destination hex
            var hexesInBewtween = Hex.HexesInBetween(end, startHex);
            if (hexesInBewtween.Count > 1)
            {
                for (int i = 1; i < hexesInBewtween.Count; i++)
                {
                    newDestination = hexesInBewtween[i];
                    if (HexIsFreeAndReachable(startHex, newDestination, map))
                    {
                        //si este hexagono esta libre y desocupado usamos este como nuevo blanco.
                        break;
                    }
                }
            }
            else //if we are here it means that the current hex we are standing is our destination and is occupied
            {
                Debug.Log("the current hex we are standing is our destination and is occupied, we return the closest free hex");
                newDestination = MapUtilities.FindClosestOpenHex(startPos, map, false);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool HexIsFreeAndReachable(Hex current, Hex end, RuntimeMap map)
    {
        

        bool reachable = UpdateReachableHexListSystem.IsReachable(current, end);
        bool free = HexIsFree(end, map);
        //Debug.Log($"the hex is free:{free} and reachable:{reachable}");
        return reachable && free;
    }

    private static bool HexIsFree(Hex hex, RuntimeMap map)
    {
        if (map.UnitsMapValues.TryGetValue(hex, out bool free))
        {
            return free;
        }

        else
        {
            return false;
        }
    }
}
