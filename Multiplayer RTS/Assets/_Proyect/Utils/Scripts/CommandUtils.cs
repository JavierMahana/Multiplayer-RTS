using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public static class CommandUtils 
{    
    public static object[] Serialize(MoveCommand command)
    {
        int entityIndex = command.Target.Index;
        int entityVersion = command.Target.Version;

        int targetQ = command.Destination.Value.q;
        int targetR = command.Destination.Value.r;

        object[] data = new object[] {entityIndex, entityVersion, targetQ, targetR };
        return data;
    }

    public static MoveCommand DeserializeMoveCommand(object[] data)
    {
        MoveCommand command = new MoveCommand()
        {
            Target = new Entity()
            {
                Index = (int)data[0],
                Version = (int)data[1]
            },
            Destination = new DestinationHex() 
            {
                Value = new Hex((int)data[2], (int)data[3])
            }
            
        };
        return command;
    }



    public static bool CommandIsValid(MoveCommand command, World world = null)
    {
        return TargetIsValid(command.Target, world);
    }

    //then the 
    private static bool TargetIsValid(Entity commandTarget, World world = null)
    {
        if (world == null)
            world = World.Active;
        
        if (world.EntityManager.HasComponent<CommandableDeathFlag>(commandTarget) && world.EntityManager.HasComponent<Commandable>(commandTarget))
        {
            return true;
        }
        else 
        {
            return false;
        }
        
        
    }
}
