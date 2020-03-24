using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Damagable : IComponentData
{

    public int InflictedDamage;    
}
