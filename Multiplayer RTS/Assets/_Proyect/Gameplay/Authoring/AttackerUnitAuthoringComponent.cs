using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

public class AttackerUnitAuthoringComponent : BaseUnitAuthoringComponent
{
    [Title("Attack values")]
    public int baseDamage = 10;
    public int ticksToAttack = 20;
    public int endlagTicks = 5;

    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        base.SetEntityComponents(entity, entityManager);

        entityManager.AddComponentData<Attacker>(entity, new Attacker() 
        {
            BaseAttackDamage = baseDamage ,
            StartUpTicks = ticksToAttack,
            EndLagTicks = endlagTicks
        });
    }
}
