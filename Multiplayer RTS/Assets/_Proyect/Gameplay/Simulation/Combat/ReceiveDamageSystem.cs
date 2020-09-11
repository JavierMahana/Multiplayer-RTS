using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class ReceiveDamageSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ReceiveDamage receiveDamage, ref Health health) => 
        {
            health.CurrentHealth = health.CurrentHealth - receiveDamage.Ammount;


            if (health.CurrentHealth <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }
            else
            {
                PostUpdateCommands.RemoveComponent<ReceiveDamage>(entity);
            }
        });


        Entities.WithNone<Health>().ForEach((Entity entity, ref ReceiveDamage receiveDamage) =>
        {
            
        });
    }
}