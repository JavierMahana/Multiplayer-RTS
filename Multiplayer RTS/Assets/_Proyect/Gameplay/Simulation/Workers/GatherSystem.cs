using System.Collections.Generic;
//using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


/// <summary>
/// Este sistema lo que hace es:
/// 1° cada ciclo de simulación agrega 1 en el contador de todas las entidades "OnGatheringResources"
/// 2° se revisa si una entidad que esta recogiendo recursos(con componente "OnGatheringResources") llego a completar su ciclo de recolección
/// (que el contador iguale o supere el maximo establecido) y le agrega el componente "WithCargo" y le remueve el componente "OnGatheringResources"
/// 3° Revisa si es que cuando una entidad recoge recursos de una fuente de recursos. esta se acaba. Para eliminar la entidad si ese es el caso.
/// 4° actualiza la cantidad de recursos que tiene la fuente(si es que no se ha acabado).
/// </summary>
[DisableAutoCreation]
public class GatherSystem : ComponentSystem
{
    private class OnGatherDataAndEntity
    {
        public OnGatherDataAndEntity(OnGatheringResources onGatheringData, Entity entity)
        {
            this.onGatheringData = onGatheringData;
            this.entity = entity;
        }

        public OnGatheringResources onGatheringData;
        public Entity entity;
    }


    //este sistema esta super mal escrito en sentido de redibilidad.
    protected override void OnUpdate()
    {


        Entities.ForEach((Entity entity, ref OnGatheringResources gatherer) =>
        {
            gatherer.progressCount++;
        });




        var ResourceEntityToItsGatherers = new Dictionary<Entity, List<OnGatherDataAndEntity>>();
        Entities.ForEach((Entity entity, ref OnGatheringResources gatherer) => 
        {
            var resEntity = gatherer.gatheringResEntity;

            List<OnGatherDataAndEntity> gatherersOnRes;
            if (ResourceEntityToItsGatherers.TryGetValue(resEntity, out gatherersOnRes))
            {
                gatherersOnRes.Add(new OnGatherDataAndEntity(gatherer, entity));
            }
            else 
            {
                gatherersOnRes = new List<OnGatherDataAndEntity>() { new OnGatherDataAndEntity(gatherer, entity) };
                ResourceEntityToItsGatherers.Add(resEntity, gatherersOnRes);
            }
        });




        var ResourceEntityToItsData = new Dictionary<Entity, ResourceSource>();
        Entities.ForEach((Entity entity, ref ResourceSource resourceSource) => 
        {
            ResourceEntityToItsData.Add(entity, resourceSource);
            
        });






        var addCargoList = new Dictionary<Entity, WithCargo>();
        var removeOnGatheringCompList = new List<Entity>();
        var resourceDepletedList = new List<Entity>();
        var resAmmountUpdatedDict = new Dictionary<Entity, int>();

        foreach (var keyValue in ResourceEntityToItsGatherers)
        {
            var resourceEntity = keyValue.Key;
            if (! ResourceEntityToItsData.ContainsKey(resourceEntity))
            {
                Debug.LogError("theres a gatherer thats his resource no longer exists");
                continue;
            }
            var resourceData = ResourceEntityToItsData[resourceEntity];
            var gathererDatasAndEntities = keyValue.Value;

            int resRemaining = resourceData.resourcesRemaining;
            foreach (var gathererAndEntity in gathererDatasAndEntities)
            {
                var gatherer = gathererAndEntity.onGatheringData;
                Entity entity = gathererAndEntity.entity;

                bool gathererCanExtract = gatherer.progressCount >= gatherer.ticksNeededForExtraction;
                if (gathererCanExtract)
                {
                    bool resourceWillBeDepleted = resRemaining - gatherer.maxCargo <= 0;
                    if (resourceWillBeDepleted)
                    {
                        resourceDepletedList.Add(resourceEntity);

                        addCargoList.Add(entity, new WithCargo() { ammount = resRemaining, resourceType = resourceData.resourceType });
                        foreach (var gathererRe in gathererDatasAndEntities)
                        {
                            if(! removeOnGatheringCompList.Contains(gathererRe.entity))
                            {
                                removeOnGatheringCompList.Add(gathererRe.entity);
                            }
                        }
                        break;
                    }
                    else 
                    {
                        resRemaining -= gatherer.maxCargo;

                        addCargoList.Add(entity, new WithCargo() { ammount = gatherer.maxCargo, resourceType = resourceData.resourceType });
                        removeOnGatheringCompList.Add(entity);
                    }
                }

                //check for < 0 to prevent adding depleted sources to the dict.
                if(resRemaining != resourceData.resourcesRemaining && resRemaining > 0)
                {
                    resAmmountUpdatedDict.Add(resourceEntity, resRemaining);
                }
            }
        }


        foreach (var res in resourceDepletedList)
        {
            PostUpdateCommands.DestroyEntity(res);
        }

        foreach (var cargo in addCargoList)
        {
            PostUpdateCommands.AddComponent(cargo.Key, cargo.Value);
        }

        foreach (var onGather in removeOnGatheringCompList)
        {
            PostUpdateCommands.RemoveComponent<OnGatheringResources>(onGather);
        }


        Entities.ForEach((Entity entity, ref ResourceSource refSource) => 
        {
            if(resAmmountUpdatedDict.ContainsKey(entity))
            {
                refSource.resourcesRemaining = resAmmountUpdatedDict[entity];
            }
        });
    }
}