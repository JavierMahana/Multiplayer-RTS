using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Parent : ISharedComponentData
{
    public Entity ParentEntity;
}
