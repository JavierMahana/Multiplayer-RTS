using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RemoveResources : IComponentData
{
    public int team;
    public int food;
    public int wood;
    public int gold;
    public int stone;
    
}
