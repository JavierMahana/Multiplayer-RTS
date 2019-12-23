using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

//este sistema es independiente de componentes.
//lo que hace es mantener y actualizar un diccionario para ver si dos hexagonos estan conectados.
[DisableAutoCreation]
//aun no esta creada
public class UpdateReachableHexListSystem : ComponentSystem
{
    

    //testeado
    private void UpdateIslandDictionaries(RuntimeMap map)
    {
        IslandHexesDictionary.Clear();
        IslandNumberDictionary.Clear();


        var walkableHexes = new List<Hex>();
        foreach (var mapKeyValuePair in map.MovementMapValues)
        {
            if (mapKeyValuePair.Value)
            {
                walkableHexes.Add(mapKeyValuePair.Key);
            }
        }

        if (walkableHexes.Count <= 0)        
        {
            return;
        }

        int infinityLoopBreak = 0;
        int islandNumber = 0;
        while (walkableHexes.Count > 0)
        {
            Hex startingHex = walkableHexes[0];

            var openList = new List<Hex>();
            var closedList = new List<Hex>();

            openList.Add(startingHex);
            while (openList.Count > 0)
            {
                var currentHex = openList[0];
                IslandNumberDictionary.Add(currentHex, islandNumber); //IslandNumberDictionary!!!
                walkableHexes.Remove(currentHex);

                openList.Remove(currentHex);
                closedList.Add(currentHex);

                for (int i = 0; i < 6; i++)
                {
                    var neightbor = currentHex.Neightbor(i);
                    if (map.MovementMapValues.ContainsKey(neightbor))
                    {
                        if (!map.MovementMapValues[neightbor] || closedList.Contains(neightbor))
                        {
                            continue;
                        }

                        if (!openList.Contains(neightbor))
                        {                            
                            openList.Add(neightbor);
                        }                        
                    }
                }

                infinityLoopBreak++;
                if (infinityLoopBreak > 1000000)
                {
                    Debug.LogError("there is a infinite loop in the system: update reachable hex list");
                    return;
                }
            }
            IslandHexesDictionary.Add(islandNumber, closedList); //IslndHexesDictionary!!!

            islandNumber++;
        }       
    }
    //testeado
    public static bool IsReachable(Hex start, Hex end)
    {
        int startIslandNumber;
        int endIslandNumber;

        if (!IslandNumberDictionary.TryGetValue(start, out startIslandNumber))
        {
            return false;
        }
        if (!IslandNumberDictionary.TryGetValue(end, out endIslandNumber))
        {
            return false;
        }

        if (startIslandNumber == endIslandNumber)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    


    private static Dictionary<Hex, int> IslandNumberDictionary;
    //por ahora no se usa.
    private static Dictionary<int, List<Hex>> IslandHexesDictionary;

    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "Thi system requires a map to work");

        UpdateIslandDictionaries(map.map);
    }
}


//public static void UpdateIslandDictionaries(RuntimeMap map, out Dictionary<Hex, int> islandNumberDictionary, out Dictionary<int, List<Hex>> islandHexesDictionary)
//{
//    islandNumberDictionary = new Dictionary<Hex, int>();
//    islandHexesDictionary = new Dictionary<int, List<Hex>>();

//    var walkableHexes = new List<Hex>();
//    foreach (var mapKeyValuePair in map.MovementMapValues)
//    {
//        if (mapKeyValuePair.Value)
//        {
//            walkableHexes.Add(mapKeyValuePair.Key);
//        }
//    }

//    if (walkableHexes.Count <= 0)
//    {
//        return;
//    }

//    int infinityLoopBreak = 0;
//    int islandNumber = 0;
//    while (walkableHexes.Count > 0)
//    {
//        Hex startingHex = walkableHexes[0];

//        var openList = new List<Hex>();
//        var closedList = new List<Hex>();

//        openList.Add(startingHex);
//        while (openList.Count > 0)
//        {
//            var currentHex = openList[0];
//            islandNumberDictionary.Add(currentHex, islandNumber); //IslandNumberDictionary!!!
//            walkableHexes.Remove(currentHex);

//            openList.Remove(currentHex);
//            closedList.Add(currentHex);

//            for (int i = 0; i < 6; i++)
//            {
//                var neightbor = currentHex.Neightbor(i);
//                if (map.MovementMapValues.ContainsKey(neightbor))
//                {
//                    if (!map.MovementMapValues[neightbor] || closedList.Contains(neightbor))
//                    {
//                        continue;
//                    }

//                    if (!openList.Contains(neightbor))
//                    {
//                        openList.Add(neightbor);
//                    }
//                }
//            }

//            infinityLoopBreak++;
//            if (infinityLoopBreak > 1000000)
//            {
//                Debug.LogError("there is a infinite loop in the system: update reachable hex list");
//                return;
//            }
//        }
//        islandHexesDictionary.Add(islandNumber, closedList); //IslndHexesDictionary!!!

//        islandNumber++;
//    }
//}
//public static bool IsReachable(Hex start, Hex end, Dictionary<Hex, int> IslandNumberDictionary, Dictionary<int, List<Hex>> IslandHexesDictionary)
//{
//    int startIslandNumber;
//    int endIslandNumber;

//    if (!IslandNumberDictionary.TryGetValue(start, out startIslandNumber))
//    {
//        return false;
//    }
//    if (!IslandNumberDictionary.TryGetValue(end, out endIslandNumber))
//    {
//        return false;
//    }

//    if (startIslandNumber == endIslandNumber)
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}