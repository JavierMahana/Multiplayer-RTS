using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WithCargo : IComponentData
{
    public int ammount;
    public ResourceType resourceType;
}
