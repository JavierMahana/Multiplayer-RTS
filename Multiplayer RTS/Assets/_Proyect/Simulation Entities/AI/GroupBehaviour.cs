using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum Behaviour 
{
    DEFAULT = 0,

    PASSIVE,
    
    AGRESIVE
}
[Serializable]
public struct GroupBehaviour : IComponentData
{
    public Behaviour Value;
}
