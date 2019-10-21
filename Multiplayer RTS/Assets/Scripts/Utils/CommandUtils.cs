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

        float targetX = command.MoveComponent.TargetPostion.x;
        float targetY = command.MoveComponent.TargetPostion.y;
        float targetZ = command.MoveComponent.TargetPostion.z;

        object[] data = new object[] {entityIndex, entityVersion, targetX, targetY, targetZ };
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
            MoveComponent = new MovementTarget() 
            {
                TargetPostion = new float3((float)data[2], (float)data[3], (float)data[4])
            }
            
        };
        return command;
    }



    public static bool CommandIsValid(MoveCommand command, World world = null)
    {
        return TargetIsValid(command.Target, world);
    }
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
