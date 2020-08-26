using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TriggerGather : IComponentData
{
    public Hex targetResourcePos;
}
