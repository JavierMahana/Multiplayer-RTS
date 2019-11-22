using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
using FixMath.NET;

public class TPAtMouseClickSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (MapManager.ActiveMap == null || !Input.GetMouseButtonDown(0))
        {
            return;
        }
        Entities.ForEach((ref HexPosition position, ref TPAtMouseClick tP) =>
        {
            var layout = MapManager.ActiveMap.layout;
            var hex = layout.WorldToFractionalHex((FixVector2)Camera.main.ScreenToWorldPoint(new Vector3( Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane)));
            position = new HexPosition() { HexCoordinates = hex };            
        });
    }
}