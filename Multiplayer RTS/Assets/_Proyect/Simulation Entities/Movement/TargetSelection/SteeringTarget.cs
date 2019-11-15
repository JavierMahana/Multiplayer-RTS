using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct SteeringTarget : IComponentData
{
    public FractionalHex TargetPosition;
}
