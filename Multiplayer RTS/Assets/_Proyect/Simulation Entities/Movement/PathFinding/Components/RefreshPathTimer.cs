using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RefreshPathTimer : IComponentData
{
    public int TurnsWithoutRefresh;
    public int TurnsRequired;
}
