using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct GatherCommand : IComponentData
{
    public Entity Target;
    public Hex TargetPos;
}
