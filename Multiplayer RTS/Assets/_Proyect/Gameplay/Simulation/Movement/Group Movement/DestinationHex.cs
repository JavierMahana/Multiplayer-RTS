using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DestinationHex : IComponentData
{
    public Hex FinalDestination;    
}
