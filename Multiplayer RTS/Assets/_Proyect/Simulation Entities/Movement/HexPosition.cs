using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HexPosition : IComponentData
{
    public FractionalHex HexCoordinates;
    public FractionalHex PrevPosition;
}
