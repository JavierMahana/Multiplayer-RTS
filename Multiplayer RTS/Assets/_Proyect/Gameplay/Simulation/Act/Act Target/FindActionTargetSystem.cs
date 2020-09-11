using FixMath.NET;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

//this system needs a little cleanup.

//It reads from the group entities their behaviour and posibles targets.
//and it reads from the unit entities its action properties like: sight distance - act distance
//with those elements in mind it chose the closest compatible target as it's target.  
[DisableAutoCreation]
public class FindActionTargetSystem : ComponentSystem
{
    private EntityQuery m_GroupDefaultQuery;
    private EntityQuery m_OnGroupDefaultQuery;
    private EntityQuery m_GathererGroupQuery;

    protected override void OnCreate()
    {
        //1) si es que el grupo tiene el componente GroupOnGather y el buffer
        //   significa que hay que usar los targets que tengan true el parametro de GatherTarget.
        //   pero antes que eso es necesario 

        m_GroupDefaultQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[]
            { 
                typeof(Group),
                typeof(ActTargetFilters),
                typeof(BEPosibleTarget),
                typeof(GroupBehaviour) 
            },
        });
        m_OnGroupDefaultQuery = GetEntityQuery( new EntityQueryDesc()
        {
            All = new ComponentType[]
            {          
                typeof(OnGroup),
                typeof(Parent), 
                typeof(SightRange), 
                typeof(Collider), 
                typeof(HexPosition), 
                typeof(ActionAttributes)
            }
        });

        m_GathererGroupQuery = GetEntityQuery(typeof(Group), typeof(GroupOnGather), typeof(BEPosibleTarget));

    }



    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "the active map must be set before this systems updates");

        var tempTargets = new Dictionary<int, ActionTarget>();

        #region ENTITIES WITH GROUP (default)

        #region init collections
        int groupCount            = m_GroupDefaultQuery.CalculateEntityCount();        
        var groupHaveTargets      = new bool[groupCount];
        var groupIndices          = new Dictionary<int, int>(groupCount);
        var groupMemberPostionSum = new FractionalHex[groupCount];
        var groupBehaviours        = new Behaviour[groupCount];
        var groupTargetsAreForGatherers = new bool[groupCount];



        int onGroupCount         = m_OnGroupDefaultQuery.CalculateEntityCount();        
        var onGroupEntityToIndex = new Dictionary<int, int>(onGroupCount);
        var onGroupPositions     = new FractionalHex[onGroupCount];
        var onGroupActRanges     = new Fix64[onGroupCount];
        var onGroupSightRanges = new Fix64[onGroupCount];
        var onGroupRadiuses      = new Fix64[onGroupCount];
        var onGroupHaveCargo = new bool[onGroupCount];
        var onGroupIsMelee = new bool[onGroupCount];


        //Esto vincula la entidad del padre con el indice que se utiliza en los arreglos con los datos de los hijos.
        var groupMemberHashMap = new Dictionary<int, List<int>>(onGroupCount);
        var onGroupParents = new Entity[onGroupCount];


        int groupIterator = 0;
        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref GroupBehaviour behaviour) =>
        {
            var posibleTargetsBuffer        = EntityManager.GetBuffer<BEPosibleTarget>(entity);
            bool haveTargets = posibleTargetsBuffer.Length != 0 && posibleTargetsBuffer.IsCreated;

            if (haveTargets)
            {
                groupTargetsAreForGatherers[groupIterator] = posibleTargetsBuffer[0].GatherTarget;
            }
            else 
            {
                groupTargetsAreForGatherers[groupIterator] = false;
            }
            groupHaveTargets[groupIterator] = haveTargets;
            groupBehaviours[groupIterator]  = behaviour.Value;
            groupIndices.Add(entity.Index, groupIterator);
            groupIterator++;
        });

        int onGroupIterator = 0;
        Entities.WithAny<OnGroup>().ForEach((Entity entity, Parent parent, ref HexPosition hexPosition, ref SightRange sightRange, ref Collider collider, ref ActionAttributes actAttributes) =>
        {
            onGroupPositions[onGroupIterator]   = hexPosition.HexCoordinates;
            onGroupActRanges[onGroupIterator]   = actAttributes.ActRange;
            onGroupSightRanges[onGroupIterator] = sightRange.Value;
            onGroupRadiuses[onGroupIterator]    = collider.Radius;
            onGroupIsMelee[onGroupIterator] = actAttributes.Melee;

            if (EntityManager.HasComponent<WithCargo>(entity))
            {
                onGroupHaveCargo[onGroupIterator] = true;
            }
            else
            {
                onGroupHaveCargo[onGroupIterator] = false;
            }


            onGroupEntityToIndex.Add(entity.Index, onGroupIterator);            
            onGroupParents[onGroupIterator]   = parent.ParentEntity;            

            int parentEntityIndex = parent.ParentEntity.Index;
            int parentIndex = groupIndices[parentEntityIndex];
            groupMemberPostionSum[parentIndex] += hexPosition.HexCoordinates;


            List<int> sameGroupIndices;
            if (groupMemberHashMap.TryGetValue(parentEntityIndex, out sameGroupIndices))
            {
                sameGroupIndices.Add(onGroupIterator);
                groupMemberHashMap[parentEntityIndex] = sameGroupIndices;
            }
            else
            {
                sameGroupIndices = new List<int>();
                sameGroupIndices.Add(onGroupIterator);
                groupMemberHashMap.Add(parentEntityIndex, sameGroupIndices);
            }

            onGroupIterator++;
        });
        #endregion

        //sets the temporal target for all the members of a group.
        foreach (var groupMemberKeyValue in groupMemberHashMap)
        {
            var groupMemberList    = new List<int>(groupMemberKeyValue.Value);
            int memberCount        = groupMemberList.Count;

            var parentEntity       = onGroupParents[groupMemberList[0]];
            int parentEntityIndex  = parentEntity.Index;
            int parentIndex        = groupIndices[parentEntityIndex];
            var groupAveragePosition = groupMemberPostionSum[parentIndex] / memberCount;


            Behaviour parentBehaviour = groupBehaviours[parentIndex];            
            bool parentHaveTargets = groupHaveTargets[parentIndex];
            bool parentTargetIsForGatherers = groupTargetsAreForGatherers[parentIndex];

            

            if (parentHaveTargets)
            {
                var parentTargetsBuffer = EntityManager.GetBuffer<BEPosibleTarget>(parentEntity);
                Debug.Assert(parentTargetsBuffer.IsCreated && parentTargetsBuffer.Length > 0, $"The target buffer of enity:{parentEntityIndex} has a problem. iscreated:{parentTargetsBuffer.IsCreated}. lenght:{parentTargetsBuffer.Length}");

                #region FOR GATHERERS
                if (parentTargetIsForGatherers)
                {
                    foreach (var gathererIdx in groupMemberList)
                    {
                        bool haveCargo = onGroupHaveCargo[gathererIdx];
                        var position = onGroupPositions[gathererIdx];
                        var actRange = onGroupActRanges[gathererIdx];

                        bool haveTarget = false;
                        Fix64 closestTargetDistance = Fix64.MaxValue;
                        var closestTarget = new ActionTarget();

                        foreach (var posibleTarget in parentTargetsBuffer)
                        {
                            if ((posibleTarget.IsResource && !haveCargo) || (!posibleTarget.IsResource && haveCargo))
                            {
                                Fix64 distance = position.Distance(posibleTarget.Position);
                                
                                if (distance < closestTargetDistance)
                                {
                                    if (posibleTarget.OccupiesFullHex)
                                    {
                                        var reversedDirection = (position - posibleTarget.Position).NormalizedManhathan();
                                        var fullHexTargetPos = FractionalHex.GetBorderPointOfTheHex(posibleTarget.Position.Round(), reversedDirection);
                                        var adjustedPosTarget = new ActionTarget()
                                        {
                                            TargetPosition = fullHexTargetPos,
                                            TargetEntity = posibleTarget.Entity,
                                            TargetRadius = Fix64.Zero,
                                            IsUnit = posibleTarget.IsUnit,
                                            GatherTarget = posibleTarget.GatherTarget,
                                            IsResource = posibleTarget.IsResource,
                                            OccupiesFullHex = true,
                                            OccupyingHex = posibleTarget.OccupyingHex,
                                            ActTypeTarget = posibleTarget.ActTypeTarget
                                        };
                                        closestTarget = adjustedPosTarget;
                                    }
                                    else
                                    {
                                        closestTarget = posibleTarget;
                                    }

                                    closestTargetDistance = distance;
                                    haveTarget = true;
                                }
                            }
                        }
                        if (haveTarget)
                        {
                            tempTargets.Add(gathererIdx, closestTarget);
                        }

                    }
                }
                #endregion
                else
                {
                    switch (parentBehaviour)
                    {
                        case Behaviour.DEFAULT: //va al más cercano
                            SetTemporalActTargetsDefaultMethod(onGroupPositions, onGroupActRanges, onGroupSightRanges, onGroupIsMelee, tempTargets, groupMemberList, groupAveragePosition, parentTargetsBuffer);
                            break;



                        case Behaviour.PASSIVE://no hay targets de un hexagono????//no esta actualizado. D:
                            foreach (int member in groupMemberList)
                            {
                                var pos = onGroupPositions[member];
                                var actRange = onGroupActRanges[member];
                                var radius = onGroupRadiuses[member];

                                var targetsInRange = new List<ActionTarget>();
                                foreach (var posibleTarget in parentTargetsBuffer)
                                {
                                    var currDistance = posibleTarget.Position.Distance(pos);
                                    if (currDistance <= actRange + posibleTarget.Radius + radius)
                                    {
                                        targetsInRange.Add(posibleTarget);
                                    }
                                }

                                for (int i = 0; i < targetsInRange.Count; i++)
                                {
                                    var target = targetsInRange[i];
                                    if (target.IsUnit)
                                    {
                                        tempTargets.Add(member, target);
                                        break;
                                    }
                                    //last item.
                                    else if (i == targetsInRange.Count - 1)
                                    {
                                        tempTargets.Add(member, target);
                                        break;
                                    }
                                }

                            }
                            break;
                        case Behaviour.AGRESIVE:
                            SetTemporalActTargetsDefaultMethod(onGroupPositions, onGroupActRanges, onGroupSightRanges, onGroupIsMelee, tempTargets, groupMemberList, groupAveragePosition, parentTargetsBuffer);
                            break;
                        //throw new System.NotImplementedException();
                        default:
                            throw new System.NotImplementedException();
                    }
                }

            }            
        }
        #endregion


        #region adds or updates the action targets
        Entities.ForEach((Entity entity, ref ActionTarget actionTarget) =>
        {
            if (onGroupEntityToIndex.TryGetValue(entity.Index, out int index))
            {
                if (tempTargets.TryGetValue(index, out ActionTarget tempTarget))
                {
                    actionTarget = tempTarget;
                }
                else //it doesn't have a target
                {
                    if (! EntityManager.HasComponent<OnGatheringResources>(entity))//HACK: lo que pasa es que si se llena un recurso de trabajadores este no se incluye en los posibles objetivos. Por esto es posible que la lista de posibles objetivos este vacia, pero enverdad hay entridades que ya estan trabajando.
                    {
                        PostUpdateCommands.RemoveComponent<ActionTarget>(entity);
                    }
                }
            }
            else//it is not on a group, for that it can't have an action target
            {
                PostUpdateCommands.RemoveComponent<ActionTarget>(entity);
            }
        });
        Entities.WithAll<OnGroup>().WithNone<ActionTarget>().ForEach((Entity entity) =>
        {
            if (onGroupEntityToIndex.TryGetValue(entity.Index, out int index))
            {
                if (tempTargets.TryGetValue(index, out ActionTarget tempTarget))
                {
                    EntityManager.AddComponentData<ActionTarget>(entity, tempTarget);
                }
            }
            else
            {
                Debug.LogError($"This entity: {entity.Index} is on a group and its index is not on the dictionary! maybe the parent or the hexPosition component are missing");
            }
        });
        #endregion
    }





    /// <summary>
    /// El action target es el que esta más cercano a la unidad. Prioriza a las unidades antes que a las estructuras.
    /// </summary>
    private static void SetTemporalActTargetsDefaultMethod(FractionalHex[] onGroupPositions, Fix64[] onGroupActRanges, Fix64[] onGroupSightRanges, bool[]onGroupIsMelees, Dictionary<int, ActionTarget> tempTargets, List<int> groupMemberList, FractionalHex groupAveragePosition, DynamicBuffer<BEPosibleTarget> parentTargetsBuffer)
    {
        foreach (int member in groupMemberList)
        {
            //lo primero es filtrar los objetivos dependiendo del range y el mapa.
            var pos = onGroupPositions[member];
            var sightRange = onGroupSightRanges[member];
            var actRange = onGroupActRanges[member];
            bool isMelee = onGroupIsMelees[member];

            //primero se ve si los posibles blancos son alcanzables y se llena una lista
            var reachableTargets = new List<ActionTarget>();
            foreach (var posibleTarget in parentTargetsBuffer)
            {
                var positionOfAction = GetPositionOfAction(posibleTarget, pos, actRange);

                bool canReach;
                if (isMelee)
                {
                    if(posibleTarget.OccupiesFullHex)
                        canReach = MapUtilities.PathToPointIsClear(pos, posibleTarget.Position, pathIsClearEvenIfDestPointIsBlocked: true);
                    else
                        canReach = MapUtilities.PathToPointIsClear(pos, posibleTarget.Position);
                }
                else 
                {
                    canReach = MapUtilities.PathToPointIsClear(pos, positionOfAction);
                }



                if (canReach)
                {                    
                    if (!posibleTarget.OccupiesFullHex)
                    {
                        reachableTargets.Add(posibleTarget);
                    }
                    else
                    {
                        var reversedDirection = (pos - posibleTarget.Position).NormalizedManhathan();
                        var fullHexTargetPos = FractionalHex.GetBorderPointOfTheHex(posibleTarget.Position.Round(), reversedDirection);
                        var adjustedPosTarget = new ActionTarget()
                        {
                            TargetPosition = fullHexTargetPos,
                            TargetEntity = posibleTarget.Entity,
                            TargetRadius = posibleTarget.Radius,
                            IsUnit = posibleTarget.IsUnit,
                            GatherTarget = posibleTarget.GatherTarget,
                            IsResource = posibleTarget.IsResource,
                            OccupiesFullHex = posibleTarget.OccupiesFullHex,
                            OccupyingHex = posibleTarget.OccupyingHex,
                            ActTypeTarget = posibleTarget.ActTypeTarget
                        };
                        reachableTargets.Add(adjustedPosTarget);
                    }
                }
            }


            //earlyout
            if (reachableTargets.Count == 0) continue;


            #region se elige el más cercano respectivo a la posicion del grupo.
            //esto usa el average grupal para lograr lo que desea.
            //Se hace esto para repartir de cierta manera los targets más variadamente y no todos a uno
            var distanceRelativeToAverage = groupAveragePosition - pos;

            var closestTarget = reachableTargets[0];
            var closesetTargetDistance = closestTarget.TargetPosition.Distance(pos + distanceRelativeToAverage);
            bool bestTargetIsUnit = closestTarget.IsUnit;


            for (int i = 1; i < reachableTargets.Count; i++)
            {
                var currentTarget = reachableTargets[i];

                //Acá se hace una distincion entre unidades y estructuras.(preferencias)
                if(currentTarget.IsUnit)
                {
                    var currDistance = currentTarget.TargetPosition.Distance(pos + distanceRelativeToAverage);
                    //si la mejor unidad es una estructura, la unidad actual reemplaza a esa estructura automaticamente como el mejor target.
                    if (!bestTargetIsUnit)
                    {
                        closesetTargetDistance = currDistance;
                        closestTarget = currentTarget;
                        bestTargetIsUnit = true;
                    }
                    else 
                    {
                        if (currDistance < closesetTargetDistance)
                        {
                            closesetTargetDistance = currDistance;
                            closestTarget = currentTarget;
                        }
                    }

                }
                else 
                {
                    //si es que ya hay una unidad puesta como mejor target, se ignoran todas las estructuras.
                    if (bestTargetIsUnit)
                        continue;
                    else 
                    {
                        var currDistance = currentTarget.TargetPosition.Distance(pos + distanceRelativeToAverage);
                        if (currDistance < closesetTargetDistance)
                        {
                            closesetTargetDistance = currDistance;
                            closestTarget = currentTarget;
                        }
                    }
                }

            }


            //esto ya usa la vision global edl equipo, asi que puede agregar al más cercano.
            tempTargets.Add(member, closestTarget);
            #endregion
        }
    }

    /// <summary>
    /// El punto real es el que cuenta el rango de accion que tiene la entidad, y ve si ocupa todo el hexagono o no.
    /// </summary>
    private static FractionalHex GetPositionOfAction(BEPosibleTarget initialTarget, FractionalHex entityPosition, Fix64 entityActionRange)
    {
        bool isFullHex = initialTarget.OccupiesFullHex;
        FractionalHex realTargetPos;
        if (!isFullHex)
        {
            var reversedDirection = (entityPosition - initialTarget.Position).NormalizedManhathan();
            //es el punto en el que uno realmente quiere llegar.
            realTargetPos = initialTarget.Position + reversedDirection * (entityActionRange + initialTarget.Radius); //it use the act range + the target radius.
        }
        else
        {
            var reversedDirection = (entityPosition - initialTarget.Position).NormalizedManhathan();

            realTargetPos = FractionalHex.GetBorderPointOfTheHex(initialTarget.Position.Round(), reversedDirection) + reversedDirection * Fix64.Max(entityActionRange, (Fix64)0.001);//Es necesario q tenga un offset
        }

        return realTargetPos;
    }
}