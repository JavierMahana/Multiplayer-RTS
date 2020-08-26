using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct BEResourceSource : IBufferElementData
{
    public static explicit operator BEResourceSource(ResourceSourceAndEntity buffer)
    {
        return new BEResourceSource()
        {
            entity = buffer.entity,
            position = buffer.resourceSource.position,

            maxGatherers = buffer.resourceSource.maxGatherers,
            currentGatherers = buffer.resourceSource.currentGatherers,

            resourceType = buffer.resourceSource.resourceType
        };
    }
    public Entity entity;
    public Hex position;

    public int maxGatherers;
    public int currentGatherers;

    public ResourceType resourceType;
}
