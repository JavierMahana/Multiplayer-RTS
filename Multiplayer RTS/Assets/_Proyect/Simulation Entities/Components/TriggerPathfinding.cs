using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TriggerPathfinding : IComponentData
{    
    public Hex Destination;    
}
