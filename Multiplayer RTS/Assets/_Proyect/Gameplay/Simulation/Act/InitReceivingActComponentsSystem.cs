using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class InitReceivingActComponentsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref IsActing isActing) =>
        {
            if (!EntityManager.HasComponent<EntityReceivingAnAction>(isActing.ActingEntity))
            {
                PostUpdateCommands.AddComponent<EntityReceivingAnAction>(isActing.ActingEntity);
            }
            if (!EntityManager.HasComponent<EntityReceivingAnActionSystemState>(isActing.ActingEntity))
            {
                PostUpdateCommands.AddComponent<EntityReceivingAnActionSystemState>(isActing.ActingEntity);
            }

        });

        //Entities.ForEach((Entity entity, ref IsActing isActing) =>
        //{
        //    if (!EntityManager.HasComponent<EntityReceivingAnAction>(isActing.ActingEntity))
        //    {
        //        PostUpdateCommands.AddComponent<EntityReceivingAnAction>(isActing.ActingEntity);
        //    }
        //    if (!EntityManager.HasComponent<EntityReceivingAnActionSystemState>(isActing.ActingEntity))
        //    {
        //        PostUpdateCommands.AddComponent<EntityReceivingAnActionSystemState>(isActing.ActingEntity);
        //    }

        //});
    }
}