using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;
using FixMath.NET;
using Unity.Collections;

[DisableAutoCreation]
public class InputSystem : ComponentSystem
{
    //EntityQuery commandableEntityQuerry;
    protected override void OnCreate()
    {
        //OfflineMode.SetOffLineMode(true);
        //EntityManager.CreateEntity(typeof(Simulate));   
        //commandableEntityQuerry = EntityManager.CreateEntityQuery(typeof(Commandable), typeof(CommandableDeathFlag));
    }
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var currentSelected = SelectionSystem.CurrentSelection;
            if (MapManager.ActiveMap == null)
            {
                Debug.Log("we need the active map");
                return;
            }
            else if (currentSelected == null)
            {
                Debug.Log("There is not an entity selected");
                return;
            }
            //early out if the entity is of other team or doesn't have team.
            if (! EntityManager.HasComponent<Team>(currentSelected.entity)) 
                return;
            var selectedTeam = EntityManager.GetComponentData<Team>(currentSelected.entity).Number;
            if (!GameManager.PlayerTeams.Contains(selectedTeam))
                return;

            var currentSelectedEntity = currentSelected.entity;
            if (EntityManager.HasComponent<Commandable>(currentSelectedEntity))
            {
                //here we see the default command for the commandable
                var defaultCommandType = EntityManager.GetComponentData<Commandable>(currentSelectedEntity).DeafaultCommand;

                switch (defaultCommandType)
                {
                    case CommandType.MOVE_COMMAND:
                        var moveCommand = new MoveCommand()
                        {
                            Target = currentSelectedEntity,
                            Destination = new DestinationHex() { FinalDestination = MapManager.ActiveMap.layout.PixelToHex(Input.mousePosition, Camera.main) }
                        };
                        CommandStorageSystem.TryAddLocalCommand(moveCommand, World.Active);
                        break;


                    default:
                        Debug.LogError("commandable doesn't have a valid default command");
                        break;
                }
            }
            else 
            {
                Debug.Log("The selected entity cannot recieve commands. It isn't commandable!");
            }
        }
    }
}
