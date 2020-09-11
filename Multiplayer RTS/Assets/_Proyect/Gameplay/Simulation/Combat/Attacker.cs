using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Attacker : IComponentData
{
    public int BaseAttackDamage;

    public int StartUpTicks;
    public int EndLagTicks;

    //ATACK BUNNUS.
}
