using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
//unused for the infinite aceleration units
public struct Arrive : IComponentData
{
    public Fix64 SlowDownRadius;
    public Fix64 StopRadius;
}
