using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DirectionAverage : IComponentData
{
    public FractionalHex Value;
    public FractionalHex PreviousDirection1;
    public FractionalHex PreviousDirection2;
}
