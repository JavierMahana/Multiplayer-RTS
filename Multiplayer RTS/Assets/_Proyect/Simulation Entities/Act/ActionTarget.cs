using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ActionTarget : IComponentData
{
    public Entity TargetEntity;
    public FractionalHex TargetPosition;
    public Fix64 TargetRadius;
}
