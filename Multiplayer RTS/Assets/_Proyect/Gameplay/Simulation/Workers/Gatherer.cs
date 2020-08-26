using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Gatherer : IComponentData
{
    public int maxCargo;

    public int woodChopingSpeed;
    public int farmingSpeed;
    public int goldMiningSpeed;
    public int stoneMiningSpeed;
    
}
