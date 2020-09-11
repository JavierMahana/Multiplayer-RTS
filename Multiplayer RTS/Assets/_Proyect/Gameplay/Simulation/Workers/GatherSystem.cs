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
/// PREV IMPLEMENTATION.
/// </summary>
//public struct GatherSystemStateComp : ISystemStateComponentData
//{ }
///// <summary>
///// Este sistema lo que hace es:
///// 1° cada ciclo de simulación agrega 1 en el contador de todas las entidades "OnGatheringResources"
///// 2° se revisa si una entidad que esta recogiendo recursos(con componente "OnGatheringResources") llego a completar su ciclo de recolección
///// (que el contador iguale o supere el maximo establecido) y le agrega el componente "WithCargo" y le remueve el componente "OnGatheringResources"
///// 3° Revisa si es que cuando una entidad recoge recursos de una fuente de recursos. esta se acaba. Para eliminar la entidad si ese es el caso.
///// 4° actualiza la cantidad de recursos que tiene la fuente(si es que no se ha acabado).
///// </summary>
//[DisableAutoCreation]
//public class GatherSystem : ComponentSystem
//{
//    private class OnGatherDataAndEntity
//    {
//        public OnGatherDataAndEntity(OnGatheringResources onGatheringData, Entity entity)
//        {
//            this.onGatheringData = onGatheringData;
//            this.entity = entity;
//        }

//        public OnGatheringResources onGatheringData;
//        public Entity entity;
//    }


//    protected override void OnUpdate()
//    {
//        //Entities.WithAll<GatherSystemStateComp>().WithNone<OnGatheringResources>().ForEach((Entity entity) => 
//        //{
//        //    PostUpdateCommands.RemoveComponent<GatherSystemStateComp>(entity);
//        //});
//        //Entities.WithNone<GatherSystemStateComp>().ForEach((Entity entity, ref OnGatheringResources onGathering) =>
//        //{
//        //    PostUpdateCommands.AddComponent<GatherSystemStateComp>(entity);
//        //    onGathering 
//        //});

//        var ammountOfGatherersOfResource = new Dictionary<Entity, int>();
//        int repetitions = 0;
//        Entities.ForEach((Entity entity, ref OnGatheringResources gatherer) =>
//        {
//            gatherer.progressCount++;

//            repetitions ++;
//            var resEntity = gatherer.gatheringResEntity;
//            if (ammountOfGatherersOfResource.ContainsKey(resEntity))
//            {
//                int currAmmount = ammountOfGatherersOfResource[resEntity];
//                ammountOfGatherersOfResource[resEntity] = (currAmmount + 1);
//            }
//            else
//            {
//                ammountOfGatherersOfResource.Add(resEntity, 1);
//            }
//        });

//        //UnityEngine.Debug.Log($"ammount of repetitions: {repetitions}");

//        //foreach (var item in ammountOfGatherersOfResource)
//        //{
//        //    UnityEngine.Debug.Log($"ammount of gatherers in resource entity number: {item.Key}: {item.Value} ");
//        //}


//        var ResourceEntityToItsGatherers = new Dictionary<Entity, List<OnGatherDataAndEntity>>();
//        Entities.ForEach((Entity entity, ref OnGatheringResources gatherer) => 
//        {
//            var resEntity = gatherer.gatheringResEntity;

//            List<OnGatherDataAndEntity> gatherersOnRes;
//            if (ResourceEntityToItsGatherers.TryGetValue(resEntity, out gatherersOnRes))
//            {
//                gatherersOnRes.Add(new OnGatherDataAndEntity(gatherer, entity));
//            }
//            else 
//            {
//                gatherersOnRes = new List<OnGatherDataAndEntity>() { new OnGatherDataAndEntity(gatherer, entity) };
//                ResourceEntityToItsGatherers.Add(resEntity, gatherersOnRes);
//            }
//        });




//        var ResourceEntityToItsData = new Dictionary<Entity, ResourceSource>();
//        Entities.ForEach((Entity entity, ref ResourceSource resourceSource) => 
//        {
//            ResourceEntityToItsData.Add(entity, resourceSource);
            
//        });






//        var addCargoList = new Dictionary<Entity, WithCargo>();
//        var removeOnGatheringCompList = new List<Entity>();
//        var resourceDepletedList = new List<Entity>();
//        var resAmmountUpdatedDict = new Dictionary<Entity, int>();
//        //var resCurrentGatherersUpdatedDict = new Dictionary<Entity, int>();

//        //Se revisa cada recurso que esta siendo recolectado.
//        foreach (var keyValue in ResourceEntityToItsGatherers)
//        {
//            var resourceEntity = keyValue.Key;
//            if (! ResourceEntityToItsData.ContainsKey(resourceEntity))
//            {
//                Debug.LogError("theres a gatherer thats his resource no longer exists");
//                continue;
//            }
//            var resourceData = ResourceEntityToItsData[resourceEntity];
//            var gathererDatasAndEntities = keyValue.Value;

//            int resRemaining = resourceData.resourcesRemaining;
//            bool resourceWillBeDepleted = false;

//            //se revisa cada gatherer que esta trabajando en el recurso
//            foreach (var gathererAndEntity in gathererDatasAndEntities)
//            {
//                var gatherer = gathererAndEntity.onGatheringData;
//                Entity entity = gathererAndEntity.entity;

//                bool gathererCanExtract = gatherer.progressCount >= gatherer.ticksNeededForExtraction;

//                //alcanzo la cantidad necesaria de ticks para recolectar el recurso?
//                if (gathererCanExtract)
//                {
//                    resourceWillBeDepleted = resRemaining - gatherer.maxCargo <= 0;
//                    if (resourceWillBeDepleted)
//                    {
//                        resourceDepletedList.Add(resourceEntity);

//                        addCargoList.Add(entity, new WithCargo() { ammount = resRemaining, resourceType = resourceData.resourceType });
//                        foreach (var gathererRe in gathererDatasAndEntities)
//                        {
//                            if(! removeOnGatheringCompList.Contains(gathererRe.entity))
//                            {
//                                removeOnGatheringCompList.Add(gathererRe.entity);
//                            }
//                        }
//                        break;
//                    }
//                    else 
//                    {
//                        resRemaining -= gatherer.maxCargo;

//                        addCargoList.Add(entity, new WithCargo() { ammount = gatherer.maxCargo, resourceType = resourceData.resourceType });
//                        removeOnGatheringCompList.Add(entity);

//                        if (ammountOfGatherersOfResource.ContainsKey(resourceEntity))
//                        {
//                            int prev = ammountOfGatherersOfResource[resourceEntity];
//                            ammountOfGatherersOfResource[resourceEntity] = prev - 1;
//                        }
//                        else 
//                        {
//                            ammountOfGatherersOfResource.Add(entity, resourceData.currentGatherers - 1);
//                        }
                        
//                    }
//                }
//            }
//            //check for < 0 to prevent adding depleted sources to the dict.
//            //si es que los recursos actuales son distintos luego de la extracción. El recurso debe ser actualizado. 
//            if (resRemaining != resourceData.resourcesRemaining && !resourceWillBeDepleted)
//            {
//                resAmmountUpdatedDict.Add(resourceEntity, resRemaining);
//            }
//        }

//        //elimina a los recursos que se han acabado.
//        foreach (var res in resourceDepletedList)
//        {
//            PostUpdateCommands.DestroyEntity(res);
//        }
//        //agrega el componente cargo a las entidades que terminaron de recolectar
//        foreach (var cargo in addCargoList)
//        {
//            PostUpdateCommands.AddComponent(cargo.Key, cargo.Value);
//        }
//        //A las entidades que terminaron de recolectar y a todas las que se les acabo el recurso del que estaban recolectando.
//        foreach (var onGather in removeOnGatheringCompList)
//        {
//            PostUpdateCommands.RemoveComponent<OnGatheringResources>(onGather);
//        }


//        Entities.ForEach((Entity entity, ref ResourceSource refSource) => 
//        {
//            //updates the ammount of resources remaining.
//            if(resAmmountUpdatedDict.ContainsKey(entity))
//            {
//                refSource.resourcesRemaining = resAmmountUpdatedDict[entity];
//            }
//            //updates the ammount of current gatherers.
//            if (ammountOfGatherersOfResource.ContainsKey(entity))
//            {
//                refSource.currentGatherers = ammountOfGatherersOfResource[entity];
//            }
//        });
//    }
//}



[DisableAutoCreation]
public class GatherSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref OnGatheringResources onGathering, ref Gatherer gatherer) =>
        {
            int progress = 1;
            switch (onGathering.gatheringResType)
            {
                case ResourceType.FOOD:
                    progress = max(1, gatherer.farmingSpeed);
                    break;
                case ResourceType.WOOD:
                    progress = max(1, gatherer.woodChopingSpeed);
                    break;
                case ResourceType.GOLD:
                    progress = max(1, gatherer.goldMiningSpeed);
                    break;
                case ResourceType.STONE:
                    progress = max(1, gatherer.stoneMiningSpeed);
                    break;
                default:
                    break;
            }
            onGathering.progressCount += progress;
        });

        var entitiesThatWillHaveExtract = new HashSet<Entity>();

        Entities.ForEach((Entity entity, ref OnGatheringResources onGathering, ref Gatherer gatherer) =>
        {
            var resEntity = onGathering.gatheringResEntity;

            if (entitiesThatWillHaveExtract.Contains(resEntity))
                return;

            if (onGathering.progressCount >= onGathering.ticksNeededForExtraction)
            { 
                if ((! EntityManager.HasComponent<Extract>(resEntity)) && EntityManager.HasComponent<ResourceSource>(resEntity))
                {
                    PostUpdateCommands.AddComponent<Extract>(resEntity, new Extract()
                    {
                        extractor = entity,
                        desiredAmmount = gatherer.maxCargo
                    });
                    PostUpdateCommands.RemoveComponent<OnGatheringResources>(entity);

                    entitiesThatWillHaveExtract.Add(resEntity);
                }
            }
        });
 
    }
}