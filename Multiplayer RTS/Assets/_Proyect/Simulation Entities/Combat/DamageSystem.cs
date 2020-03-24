using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class DamageSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Health health, ref Damagable damagable) => 
        {
            int maxHealth = health.MaxHealth;
            int currentHealth = health.CurrentHealth;

            int newHealth = currentHealth - damagable.InflictedDamage;
            if (newHealth > maxHealth)
            {
                newHealth = maxHealth;
            }

            health.CurrentHealth = newHealth;
            damagable.InflictedDamage = 0;
        });
    }
}