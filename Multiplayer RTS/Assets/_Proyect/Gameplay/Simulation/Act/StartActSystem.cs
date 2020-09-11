using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
public class StartActSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<IsActing>().ForEach((Entity entity, ref HexPosition hexPos, ref Team team, ref Collider collider, ref ActionTarget target, ref ActionAttributes attributes) =>
        {
            var pos = hexPos.HexCoordinates;
            var actType = target.ActTypeTarget;

            var distance = pos.Distance(target.TargetPosition);
            if (distance <= attributes.ActRange + collider.Radius + target.TargetRadius)
            {
                //UnityEngine.Debug.Log($"entity trying to start action with act type: {actType} ");

                switch (actType)
                {
                    case ActType.ATTACK:
                        if (
                        EntityManager.HasComponent<Attacker>(entity) &&
                        !EntityManager.HasComponent<Attacking>(entity) &&
                        EntityManager.HasComponent<Health>(target.TargetEntity))
                        {
                            var attackerData = EntityManager.GetComponentData<Attacker>(entity);
                            PostUpdateCommands.AddComponent<Attacking>(entity, new Attacking()
                            {
                                currentTick = 0,
                                tickToExecuteDamage = attackerData.StartUpTicks,
                                tickToEndAttack = attackerData.StartUpTicks + attackerData.EndLagTicks,
                                target = target.TargetEntity,
                                hasInflictedDamage = false
                            });
                            PostUpdateCommands.AddComponent<IsActing>(entity, new IsActing()
                            {
                                ActType = ActType.ATTACK,
                                ActingEntity = target.TargetEntity
                            });
                        }
                        break;
                    case ActType.HEAL:
                        break;
                    case ActType.GATHER:
                        if (
                        EntityManager.HasComponent<Gatherer>(entity) &&
                        EntityManager.HasComponent<ResourceSource>(target.TargetEntity) &&
                        !EntityManager.HasComponent<OnGatheringResources>(entity))
                        {
                            var gathererData = EntityManager.GetComponentData<Gatherer>(entity);
                            var resData = EntityManager.GetComponentData<ResourceSource>(target.TargetEntity);
                            PostUpdateCommands.AddComponent<OnGatheringResources>(entity, new OnGatheringResources()
                            {
                                gatheringResEntity = target.TargetEntity,
                                gatheringResType = resData.resourceType,
                                resPosition = target.TargetPosition,
                                maxCargo = gathererData.maxCargo,
                                progressCount = 0,
                                ticksNeededForExtraction = resData.ticksForExtraction
                            });
                            PostUpdateCommands.AddComponent<IsActing>(entity, new IsActing()
                            {
                                ActType = ActType.GATHER,
                                ActingEntity = target.TargetEntity
                            });
                        }
                        break;


                    case ActType.STORE:
                        if (
                        EntityManager.HasComponent<WithCargo>(entity) &&
                        EntityManager.HasComponent<ResourceDropPoint>(target.TargetEntity))
                        {
                            var cargo = EntityManager.GetComponentData<WithCargo>(entity);
                            var resourcesToAdd = new AddResources() { food = 0, wood = 0, gold = 0, stone = 0 };
                            switch (cargo.resourceType)
                            {
                                case ResourceType.FOOD:
                                    resourcesToAdd.food = cargo.ammount;
                                    break;
                                case ResourceType.WOOD:
                                    resourcesToAdd.wood = cargo.ammount;
                                    break;
                                case ResourceType.GOLD:
                                    resourcesToAdd.gold = cargo.ammount;
                                    break;
                                case ResourceType.STONE:
                                    resourcesToAdd.stone = cargo.ammount;
                                    break;
                                default:
                                    break;
                            }

                            PostUpdateCommands.AddComponent<AddResources>(entity, resourcesToAdd);
                            PostUpdateCommands.RemoveComponent<WithCargo>(entity);
                        }
                        else if (!EntityManager.HasComponent<ResourceDropPoint>(target.TargetEntity))
                        {
                            UnityEngine.Debug.LogError("YOU ARE TRYING TO DROP A RESOURCE IN A ENTITY THAT DOESN'T HAVE A RESOURCE DROP POINT.");
                        }
                        break;
                    default:
                        break;
                }
            }
        });
    }
}