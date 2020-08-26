using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class BlockMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var activeMap = MapManager.ActiveMap;
        if (activeMap == null)
        { Debug.LogError("Active map needed for the collision system"); return; }
        var map = activeMap.map;

        Entities.WithNone<BlockMovementSysComp, ExcludeFromSimulation>().ForEach((Entity entity, ref BlockMovement block) =>
        {
            Hex position = block.position;
            bool posValue;
            if (!map.MovementMapValues.TryGetValue(position, out posValue))
            {
                Debug.LogError($"Cannot register the block movement entity in {position} if it is not inside the map!");
                return;
            }

            if (!posValue)
            {
                Debug.LogError($"You cannot block the movement of a already ocupied position: {position}!");
            }

            map.SetMovementMapValue(position, false);
            PostUpdateCommands.AddComponent<BlockMovementSysComp>(entity, new BlockMovementSysComp() { position = position });
        });



        Entities.WithNone<BlockMovement>().ForEach((Entity entity, ref BlockMovementSysComp sysComp) =>
        {
            map.SetMovementMapValue(sysComp.position, true);
            PostUpdateCommands.RemoveComponent<BlockMovementSysComp>(entity);
        });
    }
}
