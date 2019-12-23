using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using FixMath.NET;

[Serializable]
public struct BEPosibleTarget : IBufferElementData
{
    public static implicit operator ActionTarget(BEPosibleTarget target)
    {
        return new ActionTarget() { TargetEntity = target.Entity, TargetPosition = target.Position, TargetRadius = target.Radius };
    }

    public Entity Entity;
    public FractionalHex Position;
    public Fix64 Radius;
}
