using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct OnGatheringResources : IComponentData
{
    public int progressCount;
    public int ticksNeededForExtraction;

    public int maxCargo;

    public Entity gatheringResEntity;
    public ResourceType gatheringResType;
}
