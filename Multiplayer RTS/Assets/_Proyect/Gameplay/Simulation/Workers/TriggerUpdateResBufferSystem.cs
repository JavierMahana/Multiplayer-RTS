using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
//using UnityEngine;

[DisableAutoCreation]
public class TriggerUpdateResBufferSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        bool triggerOnGoldMiners = false;
        bool triggerOnStoneMiners = false;
        bool triggerOnFoodCollecters = false;
        bool triggerOnWoodChopers = false;

        Entities.WithNone<RegisteredResourceSystemComp>().ForEach((Entity entity, ref ResourceSource res) => 
        {
            PostUpdateCommands.AddComponent(entity, new RegisteredResourceSystemComp() { Type = res.resourceType });
            switch (res.resourceType)
            {
                case ResourceType.FOOD:
                    triggerOnFoodCollecters = true;
                    break;
                case ResourceType.WOOD:
                    triggerOnWoodChopers = true;
                    break;
                case ResourceType.GOLD:
                    triggerOnGoldMiners = true;
                    break;
                case ResourceType.STONE:
                    triggerOnStoneMiners = true;
                    break;
                default:
                    UnityEngine.Debug.LogError("NOT VALID RESOURCE TYPE is being registered.");
                    break;
            }
        });
        Entities.WithNone<ResourceSource>().ForEach((Entity entity, ref RegisteredResourceSystemComp registrationRes) =>
        {
            switch (registrationRes.Type)
            {
                case ResourceType.FOOD:
                    triggerOnFoodCollecters = true;
                    break;
                case ResourceType.WOOD:
                    triggerOnWoodChopers = true;
                    break;
                case ResourceType.GOLD:
                    triggerOnGoldMiners = true;
                    break;
                case ResourceType.STONE:
                    triggerOnStoneMiners = true;
                    break;
                default:
                    UnityEngine.Debug.LogError("NOT VALID REGISTERED RESOURCE TYPE is being destroyed.");
                    break;
            }
            PostUpdateCommands.RemoveComponent<RegisteredResourceSystemComp>(entity);
        });


        if (triggerOnGoldMiners || triggerOnStoneMiners || triggerOnWoodChopers || triggerOnFoodCollecters)
        {
            Entities.WithAll<BEResourceSource>().ForEach((Entity entity, ref GroupOnGather group) => 
            {
                switch (group.GatheringResourceType)
                {
                    case ResourceType.FOOD:
                        if (triggerOnFoodCollecters)
                        {
                            PostUpdateCommands.AddComponent<UpdateResourceBuffer>(entity);
                        }
                        break;
                    case ResourceType.WOOD:
                        if (triggerOnWoodChopers)
                        {
                            PostUpdateCommands.AddComponent<UpdateResourceBuffer>(entity);
                        }
                        break;
                    case ResourceType.GOLD:
                        if (triggerOnGoldMiners)
                        {
                            PostUpdateCommands.AddComponent<UpdateResourceBuffer>(entity);
                        }
                        break;
                    case ResourceType.STONE:
                        if (triggerOnStoneMiners)
                        {
                            PostUpdateCommands.AddComponent<UpdateResourceBuffer>(entity);
                        }
                        break;
                    default:
                        break;
                }
            });
        }

    }
}