using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using FixMath.NET;

[Serializable]
public struct Radius : IComponentData
{
    public Fix64 Value;
}
