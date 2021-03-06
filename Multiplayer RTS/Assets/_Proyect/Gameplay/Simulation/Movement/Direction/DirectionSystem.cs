﻿using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
//sets the movement direction of the moving entities. it depends on the translation system
public class DirectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref DirectionAverage directionAverage, ref HexPosition position) => 
        {
            var movement = position.HexCoordinates - position.PrevPosition;
            var currentTurnDirection = movement.Lenght() <= Fix64.Zero ?
            new FractionalHex(Fix64.Zero, Fix64.Zero) : movement.NormalizedManhathan();

            directionAverage.Value = (currentTurnDirection + directionAverage.PreviousDirection1 + directionAverage.PreviousDirection2) / 3;
            directionAverage.PreviousDirection2 = directionAverage.PreviousDirection1;
            directionAverage.PreviousDirection1 = currentTurnDirection;
        });
    }
}