using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;
using FixMath.NET;
using Unity.Collections;
//implementar
public class InputSystem : ComponentSystem
{
    EntityQuery commandableEntityQuerry;
    protected override void OnCreate()
    {
        //OfflineMode.SetOffLineMode(true);
        //EntityManager.CreateEntity(typeof(Simulate));   
        commandableEntityQuerry = EntityManager.CreateEntityQuery(typeof(Commandable), typeof(CommandableDeathFlag));
    }
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (MapManager.ActiveMap == null)
            {
                Debug.Log("we need the active map");
                return;
            }

            var entities = commandableEntityQuerry.ToEntityArray(Allocator.TempJob);
            if (!(entities.IsCreated && entities.Length > 0))
            {
                Debug.Log("there aren't commandable querries");
                return;
            }
            var entityToCommand = entities[0];

            Debug.Log("adding a command");
            var worldPosOfMouse = (FixVector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            var moveCommand = new MoveCommand() {
                Target = entityToCommand,
                Destination = new DestinationHex() { Value = MapManager.ActiveMap.layout.WorldToHex(worldPosOfMouse) }             
            };
            CommandStorageSystem.TryAddLocalCommand(moveCommand, World.Active);

            entities.Dispose();
        }
    }
}
