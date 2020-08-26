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


//probable error de syncronización: cuando se loopea por todo los numeros posibles de int.
//luego se volvera al inicio y eso puede generar errores(esto si cada turno de lockstep se demora 0.2s llegaria luego de 120 hrs aprox).
[Serializable]
public struct CommandStorageSystemState : ISystemStateComponentData { }



public class CommandStorageSystem : ComponentSystem
{
    private static bool logg = false;




    //los commandos volatiles son aquellos que aun no son encolados para x turno.
    //puede existir un solo commando de cada tipo por entidad
    private static Dictionary<Entity, MoveCommand> volatileMoveCommands                       = new Dictionary<Entity, MoveCommand>();
    private static Dictionary<Entity, ChangeBehaviourCommand> volatileChangeBehaviourCommands = new Dictionary<Entity, ChangeBehaviourCommand>();
    private static Dictionary<Entity, GatherCommand> volatileGatherCommands                   = new Dictionary<Entity, GatherCommand>();






    //there will be multiples dictionaries one for each command type with the queued commands for each lockstep turn
    public static Dictionary<int, List<MoveCommand>> QueuedMoveCommands { get; private set; }                       = new Dictionary<int, List<MoveCommand>>();
    public static Dictionary<int, List<ChangeBehaviourCommand>> QueuedChangeBehaviourCommands { get; private set; } = new Dictionary<int, List<ChangeBehaviourCommand>>();
    public static Dictionary<int, List<GatherCommand>> QueuedGatherCommands { get; private set; } = new Dictionary<int, List<GatherCommand>>();


    //----------------Esta es la funcion por la cual se ingresan commandos (de este cliente) al systema....................

    /// <summary>
    /// Esta es la funcion que se usa para ingresar commandos al sistema de commandos por medio de la lista volatil
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
    public static bool TryAddLocalCommand(ChangeBehaviourCommand command, World world)
    {
        if (CommandUtils.CommandIsValid(command, world))
        {
            if (volatileChangeBehaviourCommands.ContainsKey(command.Target))
            {
                //el commando es igual al que ya esta almacenado
                if (volatileChangeBehaviourCommands[command.Target].Equals(command))
                {
                    return false;
                }
                else
                {
                    volatileChangeBehaviourCommands[command.Target] = command;
                    return true;
                }
            }
            else
            {
                volatileChangeBehaviourCommands.Add(command.Target, command);
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    public static bool TryAddLocalCommand(GatherCommand command, World world)
    {
        if (CommandUtils.CommandIsValid(command, world))
        {
            if (volatileGatherCommands.ContainsKey(command.Target))
            {
                //el commando es igual al que ya esta almacenado
                if (volatileGatherCommands[command.Target].Equals(command))
                {
                    return false;
                }
                else
                {
                    volatileGatherCommands[command.Target] = command;
                    return true;
                }
            }
            else
            {
                volatileGatherCommands.Add(command.Target, command);
                return true;
            }
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// Esta es la funcion por la que se ingresan commandos externos al systema
    /// </summary>    
    public static void QueueNetworkedCommands(int turnToQueue, MoveCommand[] moveCommands, ChangeBehaviourCommand[] changeBehaviourCommands, GatherCommand[] gatherCommands)
    {
        if (moveCommands != null)
        {
            InsertObjectsToDictionaryAtKey(turnToQueue, QueuedMoveCommands, moveCommands);
        }
        if (changeBehaviourCommands != null)
        {
            InsertObjectsToDictionaryAtKey(turnToQueue, QueuedChangeBehaviourCommands, changeBehaviourCommands);
        }
        if(gatherCommands != null)
        {
            InsertObjectsToDictionaryAtKey(turnToQueue, QueuedGatherCommands, gatherCommands);
        }
        
    }
    /// <summary>
    /// La serializacion de todos los commandos volatiles sirve para mandarlos a la network
    /// </summary>
    /// <returns>object[] -> object[](de commandos)</returns>
    public static object[] GetAllVolatileCommandsSerialized()
    {    
        //null check is important

        MoveCommand[] moveCommands = CommandDictionaryToArray(volatileMoveCommands);
        object[] moveCommandsSerialized;
        if (moveCommands != null)
        {
            moveCommandsSerialized = new object[moveCommands.Length];
            for (int i = 0; i < moveCommands.Length; i++)
            {
                moveCommandsSerialized[i] = CommandUtils.Serialize(moveCommands[i]);
            }
        }
        else
            moveCommandsSerialized = null; 
        
        

        ChangeBehaviourCommand[] changeBehaviourCommands = CommandDictionaryToArray(volatileChangeBehaviourCommands);
        object[] changeBehaviourCommandsSerialized;
        if (changeBehaviourCommands != null)
        {
            changeBehaviourCommandsSerialized = new object[changeBehaviourCommands.Length];
            for (int i = 0; i < changeBehaviourCommands.Length; i++)
            {
                changeBehaviourCommandsSerialized[i] = CommandUtils.Serialize(changeBehaviourCommands[i]);
            }
        }
        else
            changeBehaviourCommandsSerialized = null;



        GatherCommand[] gatherCommands = CommandDictionaryToArray(volatileGatherCommands);
        object[] gatherCommandsSerialized;
        if (gatherCommands != null)
        {
            gatherCommandsSerialized = new object[gatherCommands.Length];
            for (int i = 0; i < gatherCommands.Length; i++)
            {
                gatherCommandsSerialized[i] = CommandUtils.Serialize(gatherCommands[i]);
            }
        }
        else
            gatherCommandsSerialized = null;


        return new object[]
        {
            moveCommandsSerialized,
            changeBehaviourCommandsSerialized,
            gatherCommandsSerialized
        };
            
    }
    public static bool AreVolatileCommands()
    {
        if (volatileMoveCommands.Count > 0)
            return true;
        else if (volatileChangeBehaviourCommands.Count > 0)
            return true;
        else if (volatileGatherCommands.Count > 0)
            return true;
        else
            return false;
    }
    public static void QueueVolatileCommands(int turnToQueue)
    {
        int count = CommandDictionaryToList(volatileMoveCommands) == null ? 0 : CommandDictionaryToList(volatileMoveCommands).Count;
        if(count != 0 && logg) Debug.Log($"Queueing move {count} command(s) at turn: {MainSimulationLoopSystem.CurrentLockstepTurn}.");


        //inject volatile commands to the list.
        //and clear the list afterwards.
        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedMoveCommands, CommandDictionaryToList(volatileMoveCommands));
        volatileMoveCommands.Clear();

        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedChangeBehaviourCommands, CommandDictionaryToList(volatileChangeBehaviourCommands));
        volatileChangeBehaviourCommands.Clear();


        InsertObjectsToDictionaryAtKey(turnToQueue, QueuedGatherCommands, CommandDictionaryToList(volatileGatherCommands));
        volatileGatherCommands.Clear();
        //other commands.
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


        volatileChangeBehaviourCommands.Clear();
        QueuedChangeBehaviourCommands.Clear();


        volatileGatherCommands.Clear();
        QueuedGatherCommands.Clear();
    }

}