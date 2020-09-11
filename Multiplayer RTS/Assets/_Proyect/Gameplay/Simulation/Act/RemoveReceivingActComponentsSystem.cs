using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class RemoveReceivingActComponentsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {

        Entities.WithAll<EntityReceivingAnAction, EntityReceivingAnActionSystemState>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.RemoveComponent<EntityReceivingAnAction>(entity);
            PostUpdateCommands.RemoveComponent<EntityReceivingAnActionSystemState>(entity);

        });

    }
}