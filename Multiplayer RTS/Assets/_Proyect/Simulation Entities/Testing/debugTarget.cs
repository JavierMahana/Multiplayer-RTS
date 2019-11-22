using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct debugTarget : IComponentData
{
    public float3 hex;
    
}
