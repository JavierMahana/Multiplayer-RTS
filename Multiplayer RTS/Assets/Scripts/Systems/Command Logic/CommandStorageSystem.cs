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
    private static Dictionary<Entity, MoveCommand> volatileMoveCommands = new Dictionary<Entity, MoveCommand>();

    //there will be multiples dictionaries with the queued commands for each lockstep turn
    public static Dictionary<int, List<MoveCommand>> QueuedMoveCommands { get; private set; } = new Dictionary<int, List<MoveCommand>>();


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
    
    //public static object[] GetAllVolatileCommands()
    //{
    //    return new object[]
    //    {
    //        CommandDictionaryToArray(volatileMoveCommands)
    //    };
    //}
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
        if(count != 0 && logg) Debug.Log($"Queueing {count} command(s) at turn: {LockstepTurnFinisherSystem.LockstepTurnCounter}.");
        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedMoveCommands, CommandDictionaryToList(volatileMoveCommands));
        volatileMoveCommands.Clear();

    }
    public static bool TryAddLocalCommand(MoveCommand command, World world)     
    {
        if (CommandUtils.CommandIsValid(command, world))
        {
            if (volatileMoveCommands.ContainsKey(command.Target))
            {                
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