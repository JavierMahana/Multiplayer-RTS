using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class UpdateGathererAmmountSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var ammountOfGatherersOfResource = new Dictionary<Entity, int>();
        Entities.ForEach((ref OnGatheringResources onGathering) =>
        {
            var resEntity = onGathering.gatheringResEntity;
            if (ammountOfGatherersOfResource.ContainsKey(resEntity))
            {
                int currAmmount = ammountOfGatherersOfResource[resEntity];
                ammountOfGatherersOfResource[resEntity] = (currAmmount + 1);
            }
            else
            {
                ammountOfGatherersOfResource.Add(resEntity, 1);
            }
        });


        Entities.ForEach((Entity entity, ref ResourceSource resource) => 
        {
            if (ammountOfGatherersOfResource.ContainsKey(entity))
            {
                resource.currentGatherers = ammountOfGatherersOfResource[entity];
            }
            else
            {
                resource.currentGatherers = 0;
            }
        });
    }
}