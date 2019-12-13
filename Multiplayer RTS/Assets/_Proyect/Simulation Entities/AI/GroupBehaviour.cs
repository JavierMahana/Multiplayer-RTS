using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct GroupBehaviour : IComponentData
{
    public bool ActOnTeamates;
    public bool ActOnEnemies;

    public Fix64 SightDistance;
}
