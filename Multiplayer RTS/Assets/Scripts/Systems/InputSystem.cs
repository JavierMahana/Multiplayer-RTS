using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System;
using Unity.Mathematics;
//implementar
public class InputSystem : ComponentSystem
{
    private Entity entity;
    protected override void OnCreate()
    {
        //OfflineMode.SetOffLineMode(true);
        //EntityManager.CreateEntity(typeof(Simulate));
        entity = EntityManager.CreateEntity(typeof(MovementTarget), typeof(Commandable),typeof(CommandableDeathFlag));
    }
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("adding a command");
            var moveCommand = new MoveCommand() { Target = entity, MoveComponent = new MovementTarget() { TargetPostion = (float3)Input.mousePosition } };
            CommandStorageSystem.TryAddLocalCommand(moveCommand, World.Active);
        }
    }
}
