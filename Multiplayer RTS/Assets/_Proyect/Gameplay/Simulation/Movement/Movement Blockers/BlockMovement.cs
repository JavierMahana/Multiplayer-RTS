using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
///component that makes a entity mark the position of ther map as occupied.
///when the entity is deleted the map position is unblocked.
public struct BlockMovement : IComponentData
{
    public Hex position;
}
