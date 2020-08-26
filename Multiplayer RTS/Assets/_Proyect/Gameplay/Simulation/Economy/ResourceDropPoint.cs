using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ResourceDropPoint : IComponentData
{
    public bool CanDropGold;
    public bool CanDropStone;
    public bool CanDropWood;
    public bool CanDropFood;
}
