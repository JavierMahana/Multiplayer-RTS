using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ResourceSource : IComponentData
{
    public Hex position;

    public int maxGatherers;
    public int currentGatherers;

    public int resourcesRemaining;
    public ResourceType resourceType;

    public int ticksForExtraction;
}
