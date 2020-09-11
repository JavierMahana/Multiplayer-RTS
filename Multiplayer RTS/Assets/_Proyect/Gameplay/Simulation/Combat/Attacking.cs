using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Attacking : IComponentData
{
    public int currentTick;
    public int tickToExecuteDamage;
    public int tickToEndAttack;

    public Entity target;
    public bool hasInflictedDamage;
}
