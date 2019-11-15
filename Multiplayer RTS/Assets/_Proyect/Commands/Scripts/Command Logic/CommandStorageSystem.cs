using System.Collections.Generic;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct CommandStorageSystemState : ISystemStateComponentData { }



public class CommandStorageSystem : ComponentSystem
{
    private static bool logg = false;
    //los commandos volatiles son aquellos que aun no son encolados para x turno.
    //puede existir un solo commando de cada tipo por entidad
    private static Dictionary<Entity, MoveCommand> volatileMoveCommands = new Dictionary<Entity, MoveCommand>();

    //there will be multiples dictionaries one for each command type with the queued commands for each lockstep turn
    public static Dictionary<int, List<MoveCommand>> QueuedMoveCommands { get; private set; } = new Dictionary<int, List<MoveCommand>>();

    /// <summary>
    /// La serializacion de todos los commandos volatiles sirve para mandarlos a la network
    /// </summary>
    /// <returns>object[] -> object[](de commandos)</returns>
    public static object[] GetAllVolatileCommandsSerialized()
    {


        MoveCommand[] moveCommands = CommandDictionaryToArray(volatileMoveCommands);
        object[] moveCommandsSerialized = new object[moveCommands.Length];
        for (int i = 0; i < moveCommands.Length; i++)
        {
            moveCommandsSerialized[i] = CommandUtils.Serialize(moveCommands[i]);
        }


        
        return new object[]
        {
            moveCommandsSerialized

        };
            
    }
    

    public static bool AreVolatileCommands()
    {
        if (volatileMoveCommands.Count > 0)
            return true;
        else
            return false;
    }
    public static void QueueNetworkedCommands(int turnToQueue, MoveCommand[] moveCommands) 
    {
        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedMoveCommands, moveCommands);
    }
    

    public static void QueueVolatileCommands(int turnToQueue)
    {
        int count = CommandDictionaryToList(volatileMoveCommands) == null ? 0 : CommandDictionaryToList(volatileMoveCommands).Count;
        if(count != 0 && logg) Debug.Log($"Queueing {count} command(s) at turn: {MainSimulationLoopSystem.CurrentLockstepTurn}.");


        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedMoveCommands, CommandDictionaryToList(volatileMoveCommands));
        volatileMoveCommands.Clear();

    }
    /// <summary>
    /// Esta es la funcion que se usa ingresar commandos al sistema commandos a la lista volatil
    /// </summary>
    /// <returns> falso si el commando es invalido o esta repetido</returns>
    public static bool TryAddLocalCommand(MoveCommand command, World world)     
    {
        if (CommandUtils.CommandIsValid(command, world))
        {
            if (volatileMoveCommands.ContainsKey(command.Target))
            {             
                //el commando es igual al que ya esta almacenado
                if (volatileMoveCommands[command.Target].Equals(command))
                {
                    return false;
                }
                else 
                {
                    volatileMoveCommands[command.Target] = command;
                    return true;
                }
            }
            else 
            {
                volatileMoveCommands.Add(command.Target, command);
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Esta función lo que hace es que en un diccionario que se entrega(de cualquiera de los queued)
    /// agrega commandos a la key entregada o crea una entrada en el diccionario para esa key
    /// </summary>    
    private static void InsertObjectsToDictionaryAtKey<T>(int key, Dictionary<int, List<T>> dictionary, IEnumerable<T> collectionOfObjectsToInject)
    {
        if (collectionOfObjectsToInject == null)
        {
            return;
        }

        if (dictionary.TryGetValue(key, out List<T> currentContent))
        {
            currentContent.AddRange(collectionOfObjectsToInject);
        }
        else
        {
            dictionary.Add(key, collectionOfObjectsToInject.ToList());
        }
    }
    private static List<T> CommandDictionaryToList<T>(Dictionary<Entity, T> dictionary )
    {
        List<T> returnList = new List<T>();
        foreach (var entry in dictionary)
        {
            returnList.Add(entry.Value);
        }

        if (returnList.Count == 0)
        {
            return null;
        }
        else
        {
            return returnList;
        }
    }
    private static T[] CommandDictionaryToArray<T>(Dictionary<Entity, T> dictionary)
    {
        if (dictionary.Count == 0)
        {
            return null;
        }

        T[] returnArray = new T[dictionary.Count];
        int i = 0;
        foreach (var entry in dictionary)
        {
            returnArray[i] = entry.Value;
            i++;
        }

        return returnArray;
    }
    protected override void OnUpdate()
    {
        //Entities.WithAll<CommandStorageSystemState, Simulate>().ForEach((Entity entity) => {});

        Entities.WithNone<CommandStorageSystemState>().WithAll<Simulate>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<CommandStorageSystemState>(entity);
        });
        Entities.WithNone<Simulate>().WithAll<CommandStorageSystemState>().ForEach((Entity entity) =>
        {
            ResetState();
            EntityManager.RemoveComponent<CommandStorageSystemState>(entity);
        });
    }

    private void ResetState()
    {
        volatileMoveCommands.Clear();
        QueuedMoveCommands.Clear();
    }

}