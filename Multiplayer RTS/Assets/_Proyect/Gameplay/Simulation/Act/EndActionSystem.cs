using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

/// <summary>
/// Esta clase lo que hace es eliminar el componente on Act de las entidades que ya terminaron de actuar.
/// Tambien elmina el componente act y sus complementos de las entidades que estaban trabajando en una entidad que fue eliminada.
/// </summary>
[DisableAutoCreation]
public class EndActionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var deletedEntitiesReceivingAnAction = new HashSet<Entity>();
        Entities.WithAll<EntityReceivingAnActionSystemState>().WithNone<EntityReceivingAnAction>().ForEach((Entity entity) => 
        {
            deletedEntitiesReceivingAnAction.Add(entity);
        });


        Entities.ForEach((Entity entity, ref IsActing isActing) =>
        {
            var actingEntity = isActing.ActingEntity;
            var actType = isActing.ActType;

            if (deletedEntitiesReceivingAnAction.Contains(actingEntity))
            {
                UnityEngine.Debug.Log("An entity is considered dead!");
                switch (actType)
                {
                    case ActType.ATTACK:
                        PostUpdateCommands.RemoveComponent<IsActing>(entity);
                        PostUpdateCommands.RemoveComponent<Attacking>(entity);
                        break;
                    case ActType.HEAL:
                        break;
                    case ActType.GATHER:
                        PostUpdateCommands.RemoveComponent<IsActing>(entity);
                        PostUpdateCommands.RemoveComponent<OnGatheringResources>(entity);
                        break;
                    case ActType.STORE:
                        break;
                    default:
                        break;
                }
            }
            else 
            {
                switch (actType)
                {
                    case ActType.ATTACK:
                        if (!EntityManager.HasComponent<Attacking>(entity))
                        {
                            PostUpdateCommands.RemoveComponent<IsActing>(entity);
                        }
                        break;
                    case ActType.HEAL:
                        break;
                    case ActType.GATHER:
                        if (!EntityManager.HasComponent<OnGatheringResources>(entity))
                        {
                            PostUpdateCommands.RemoveComponent<IsActing>(entity);
                        }
                        break;
                    case ActType.STORE:
                        break;
                    default:
                        break;
                }
            }
        });



        Entities.WithAll<EntityReceivingAnActionSystemState>().WithNone<EntityReceivingAnAction>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.RemoveComponent<EntityReceivingAnActionSystemState>(entity);
        });
    }
}