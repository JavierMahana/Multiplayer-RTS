using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
//This tick is going to be extracted.
public struct Extract : IComponentData
{
    public Entity extractor;
    public int desiredAmmount;
}
