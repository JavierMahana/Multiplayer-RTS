using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class ExtractResourceSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Extract extract, ref ResourceSource resource) => 
        {
            var gathererEntity = extract.extractor;
            int resourcesToExtract;
            bool resourceIsGoingToBeDepleted = false;




            if (resource.resourcesRemaining <= extract.desiredAmmount)
            {
                resourceIsGoingToBeDepleted = true;
                resourcesToExtract = resource.resourcesRemaining;
            }
            else 
            {
                resourcesToExtract = extract.desiredAmmount;
            }




            if (resourceIsGoingToBeDepleted)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }
            else 
            {
                resource.resourcesRemaining -= resourcesToExtract;
            }





            if (EntityManager.HasComponent<Gatherer>(gathererEntity) && (!EntityManager.HasComponent<WithCargo>(gathererEntity)))
            {
                PostUpdateCommands.AddComponent<WithCargo>(gathererEntity, new WithCargo()
                {
                    ammount = resourcesToExtract,
                    resourceType = resource.resourceType
                });
            }


            PostUpdateCommands.RemoveComponent<Extract>(entity);
        });
    }
}