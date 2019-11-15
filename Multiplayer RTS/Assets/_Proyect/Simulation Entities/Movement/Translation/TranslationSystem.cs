using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
//it moves the entity's hex positions usign the velocity
//it depends on the collision and the steering system
public class TranslationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref DesiredMovement turnMovement, ref HexPosition position) => 
        {
            var prevPosition = position.HexCoordinates;
            position = new HexPosition() { HexCoordinates = (prevPosition + turnMovement.Value)};
        });
    }
}