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
        return new ActionTarget() 
        {
            TargetEntity = target.Entity, 
            TargetPosition = target.Position, 
            TargetRadius = target.Radius, 
            IsUnit = target.IsUnit, 
            OccupiesFullHex = target.OccupiesFullHex,
            GatherTarget = target.GatherTarget,
            IsResource = target.IsResource,
            ActTypeTarget = target.ActTypeTarget,
            OccupyingHex = target.OccupyingHex
        };
    }

    public Entity Entity;
    public FractionalHex Position;
    public Fix64 Radius;
    public bool IsUnit;

    public bool OccupiesFullHex;
    public Hex OccupyingHex;

    public ActType ActTypeTarget;

    public bool GatherTarget;
    public bool IsResource;

}
