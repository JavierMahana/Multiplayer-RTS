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
                Hex clickHex = MapManager.ActiveMap.layout.PixelToHex(Input.mousePosition, Camera.main);


                switch (defaultCommandType)
                {
                    case CommandType.MOVE_COMMAND:
                        var moveCommand = new MoveCommand()
                        {
                            Target = currentSelectedEntity,
                            Destination = new DestinationHex() { FinalDestination = clickHex }
                        };
                        CommandStorageSystem.TryAddLocalCommand(moveCommand, World.Active);
                        break;

                    case CommandType.GATHER_COMMAND:
                        //gather si es que se cliquea a un recurso, si no solo moverse.
                        if (ResourceSourceManagerSystem.TryGetResourceAtHex(clickHex, out ResourceSourceAndEntity source))
                        {
                            var gatherCommand = new GatherCommand()
                            {
                                Target = currentSelectedEntity,
                                TargetPos = clickHex
                            };
                            CommandStorageSystem.TryAddLocalCommand(gatherCommand, World.Active);
                        }
                        else 
                        {
                            var moveCommand2 = new MoveCommand()
                            {
                                Target = currentSelectedEntity,
                                Destination = new DestinationHex() { FinalDestination = clickHex }
                            };
                            CommandStorageSystem.TryAddLocalCommand(moveCommand2, World.Active);
                        }
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
