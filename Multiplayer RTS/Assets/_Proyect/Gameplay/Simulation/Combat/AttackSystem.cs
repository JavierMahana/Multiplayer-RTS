using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class AttackSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var entitiesThatWillRecieveDamage = new HashSet<Entity>();
        Entities.ForEach((Entity entity, ref Attacker attacker, ref Attacking attacking) => 
        {
            attacking.currentTick = attacking.currentTick + 1;

            var targetEntity = attacking.target;

            //EARLYOUT
            if (entitiesThatWillRecieveDamage.Contains(targetEntity) || EntityManager.HasComponent<ReceiveDamage>(targetEntity))
                return;



            if (!attacking.hasInflictedDamage && attacking.currentTick >= attacking.tickToExecuteDamage)
            {
                PostUpdateCommands.AddComponent<ReceiveDamage>(targetEntity, new ReceiveDamage()
                {
                    Ammount = attacker.BaseAttackDamage                    
                }) ;
                entitiesThatWillRecieveDamage.Add(targetEntity);
                attacking.hasInflictedDamage = true;
            }

            if (attacking.currentTick >= attacking.tickToEndAttack)
            {
                PostUpdateCommands.RemoveComponent<Attacking>(entity);
            }
        });

        
    }
}