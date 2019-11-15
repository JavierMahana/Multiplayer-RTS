using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DesiredMovement : IComponentData
{
    public FractionalHex Value;
    
}
