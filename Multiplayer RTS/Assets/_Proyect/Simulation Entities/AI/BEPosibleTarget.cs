using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using FixMath.NET;

[Serializable]
public struct BEPosibleTarget : IBufferElementData
{
    public Entity Entity;
    public FractionalHex Position;
    public Fix64 Radius;
}
