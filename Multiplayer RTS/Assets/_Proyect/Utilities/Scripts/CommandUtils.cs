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

        int targetQ = command.Destination.FinalDestination.q;
        int targetR = command.Destination.FinalDestination.r;

        object[] data = new object[] {entityIndex, entityVersion, targetQ, targetR };
        return data;
    }
    public static object[] Serialize(ChangeBehaviourCommand command)
    {
        int entityIndex = command.Target.Index;
        int entityVersion = command.Target.Version;

        int behaviour = (int)command.NewBehaviour.Value;

        object[] data = new object[] { entityIndex, entityVersion, behaviour };
        return data;
    }
    public static object[] Serialize(GatherCommand command)
    {
        int entityIndex = command.Target.Index;
        int entityVersion = command.Target.Version;

        int targetQ = command.TargetPos.q;
        int targetR = command.TargetPos.r;

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
                FinalDestination = new Hex((int)data[2], (int)data[3])
            }
            
        };
        return command;
    }
    public static ChangeBehaviourCommand DeserializeChangeBehaviourCommand(object[] data)
    {
        ChangeBehaviourCommand command = new ChangeBehaviourCommand()
        {
            Target = new Entity()
            {
                Index = (int)data[0],
                Version = (int)data[1]
            },
            NewBehaviour = new GroupBehaviour()
            {
                Value = (Behaviour)data[2]
            }
        };
        return command;
    }
    public static GatherCommand DeserializeGatherCommand(object[] data)
    {
        GatherCommand command = new GatherCommand()
        {
            Target = new Entity()
            {
                Index = (int)data[0],
                Version = (int)data[1],
            },
            TargetPos = new Hex((int)data[2], (int)data[3]),
        };
        return command;
    }




    public static bool CommandIsValid(MoveCommand command, World world = null)
    {
        return TargetIsValid(command.Target, world);
    }
    public static bool CommandIsValid(ChangeBehaviourCommand command, World world = null)
    {
        return TargetIsValid(command.Target, world);
    }
    public static bool CommandIsValid(GatherCommand command, World world = null)
    {
        if (world == null)
            world = World.Active;

        bool gatherCompontnentsValidation;

        if (world.EntityManager.HasComponent<HasGatherer>(command.Target) && world.EntityManager.HasComponent<Team>(command.Target))
        {
            gatherCompontnentsValidation = true;
        }
        else
        {
            gatherCompontnentsValidation = false;
        }
        bool basicCommandValidation = TargetIsValid(command.Target, world);

        return gatherCompontnentsValidation && basicCommandValidation;
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
