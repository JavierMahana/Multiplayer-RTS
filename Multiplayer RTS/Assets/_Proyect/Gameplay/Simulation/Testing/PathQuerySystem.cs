using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using FixMath.NET;

public class PathQuerySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (MapManager.ActiveMap != null)
            {
                var mousePos = Input.mousePosition;
                var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
                var hex = MapManager.ActiveMap.layout.WorldToHex(new FixVector2( (Fix64)worldPos.x, (Fix64)worldPos.y));
                Entities.ForEach((Entity entity, ref MouseClickTriggerPathfinding t) => 
                {
                    PostUpdateCommands.AddComponent(entity, new TriggerPathfinding() { Destination = hex});
                });

            }
        }
    }
}