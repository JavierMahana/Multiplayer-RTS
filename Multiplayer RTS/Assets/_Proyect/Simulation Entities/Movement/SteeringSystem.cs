using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
[DisableAutoCreation]
public class SteeringSystem : ComponentSystem
{
    //como inicio lo unico que hará es mover
    protected override void OnUpdate()
    {
        //como prueba el steer es ir a toda velocidad al target
        Entities.ForEach((ref DesiredMovement desiredMovement, ref HexPosition position, ref Speed speed, ref SteeringTarget target) => 
        {
            var postionDelta = target.TargetPosition - position.HexCoordinates;
            if (postionDelta.Lenght() <= Fix64.Zero) { desiredMovement.Value = new FractionalHex(Fix64.Zero, Fix64.Zero); }
            else { desiredMovement.Value = postionDelta.Normalized() * speed.Value; }
        });
        
        //el grupo
        //en gupo
        //en refuerzo
        
    }

}