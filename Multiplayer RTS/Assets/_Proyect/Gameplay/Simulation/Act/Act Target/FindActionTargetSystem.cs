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


//ahora mismo se esta usando el valor de "Triggered" del padre para asignar el ActionTarget

//this system needs a little cleanup.
//It reads from the group entities their behaviour and posibles targets.
//and it reads from the unit entities its action properties like: sight distance - act distance
//with those elements in mind it chose the closest compatible target as it's target.  
[DisableAutoCreation]
public class FindActionTargetSystem : ComponentSystem
{
    private EntityQuery m_GroupQuery;
    private EntityQuery m_OnGroupQuery;    

    protected override void OnCreate()
    {
        m_GroupQuery = GetEntityQuery(typeof(Group), typeof(ActTargetFilters), typeof(BEPosibleTarget), typeof(GroupBehaviour));
        m_OnGroupQuery = GetEntityQuery(typeof(OnGroup), typeof(Parent), typeof(SightRange), typeof(Collider), typeof(HexPosition), typeof(ActionAttributes));        
    }



    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "the active map must be set before this systems updates");

        int groupCount            = m_GroupQuery.CalculateEntityCount();        
        var groupHaveTargets      = new bool[groupCount];
        var groupIndices          = new Dictionary<int, int>(groupCount);
        var groupMemberPostionSum = new FractionalHex[groupCount];
        var groupBehaviours        = new Behaviour[groupCount];

        int onGroupCount         = m_OnGroupQuery.CalculateEntityCount();        
        var onGroupEntityToIndex = new Dictionary<int, int>(onGroupCount);
        var onGroupPositions     = new FractionalHex[onGroupCount];
        var onGroupActRanges     = new Fix64[onGroupCount];
        var onGroupSightRanges = new Fix64[onGroupCount];
        var onGroupRadiuses      = new Fix64[onGroupCount];
        var tempTargets          = new Dictionary<int, ActionTarget>();

        var groupMemberHashMap = new Dictionary<int, List<int>>(onGroupCount);
        var onGroupParents = new Entity[onGroupCount];


        int groupIterator = 0;
        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref GroupBehaviour behaviour) =>
        {
            var posibleTargetsBuffer        = EntityManager.GetBuffer<BEPosibleTarget>(entity);            
            groupHaveTargets[groupIterator] = posibleTargetsBuffer.Length != 0 && posibleTargetsBuffer.IsCreated;
            groupBehaviours[groupIterator]  = behaviour.Value;
            groupIndices.Add(entity.Index, groupIterator);
            groupIterator++;
        });

        int onGroupIterator = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref HexPosition hexPosition, ref SightRange sightRange, ref Collider collider, ref ActionAttributes actAttributes) =>
        {
            onGroupPositions[onGroupIterator]   = hexPosition.HexCoordinates;
            onGroupActRanges[onGroupIterator]   = actAttributes.ActRange;
            onGroupSightRanges[onGroupIterator] = sightRange.Value;
            onGroupRadiuses[onGroupIterator]    = collider.Radius;

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

            if (parentHaveTargets)
            {                
                var parentTargetsBuffer = EntityManager.GetBuffer<BEPosibleTarget>(parentEntity);
                Debug.Assert(parentTargetsBuffer.IsCreated && parentTargetsBuffer.Length > 0, $"The target buffer of enity:{parentEntityIndex} has a problem. iscreated:{parentTargetsBuffer.IsCreated}. lenght:{parentTargetsBuffer.Length}");

                switch (parentBehaviour)
                {
                    case Behaviour.DEFAULT: //va al más cercano
                        SetTemporalActTargetsDefaultMethod(onGroupPositions, onGroupActRanges, onGroupSightRanges, tempTargets, groupMemberList, groupAveragePosition, parentTargetsBuffer);
                        break;



                    case Behaviour.PASSIVE:
                        foreach (int member in groupMemberList)
                        {                            
                            var pos      = onGroupPositions[member];
                            var actRange = onGroupActRanges[member];
                            var radius   = onGroupRadiuses[member];

                            foreach (var posibleTarget in parentTargetsBuffer)
                            {
                                var currDistance = posibleTarget.Position.Distance(pos);
                                if (currDistance <= actRange + posibleTarget.Radius + radius)
                                {
                                    tempTargets.Add(member, posibleTarget);
                                    break;
                                }
                            }                            
                        }
                        break;
                    case Behaviour.AGRESIVE:
                        SetTemporalActTargetsDefaultMethod(onGroupPositions, onGroupActRanges, onGroupSightRanges, tempTargets, groupMemberList, groupAveragePosition, parentTargetsBuffer);
                        break;
                        //throw new System.NotImplementedException();
                    default:
                        throw new System.NotImplementedException();
                }

                
            }            
        }


        //adds or updates the action targets
        Entities.ForEach((Entity entity, ref ActionTarget actionTarget) => 
        {
            if (onGroupEntityToIndex.TryGetValue(entity.Index, out int index))
            {
                if(tempTargets.TryGetValue(index, out ActionTarget tempTarget))                                
                {
                    actionTarget = tempTarget;                    
                }
                else //it doesn't have a target
                {
                    PostUpdateCommands.RemoveComponent<ActionTarget>(entity);
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
    }

    private static void SetTemporalActTargetsDefaultMethod(FractionalHex[] onGroupPositions, Fix64[] onGroupActRanges, Fix64[] onGroupSightRanges, Dictionary<int, ActionTarget> tempTargets, List<int> groupMemberList, FractionalHex groupAveragePosition, DynamicBuffer<BEPosibleTarget> parentTargetsBuffer)
    {
        foreach (int member in groupMemberList)
        {
            //lo primero es filtrar los objetivos dependiendo del range y su el mapa.
            var pos = onGroupPositions[member];
            var sightRange = onGroupSightRanges[member];
            var actRange = onGroupActRanges[member];

            var reachableTargets = new List<ActionTarget>();
            foreach (var posibleTarget in parentTargetsBuffer)
            {
                var reversedDirection = (pos - posibleTarget.Position).NormalizedManhathan();
                var realTargetPoint = posibleTarget.Position + reversedDirection * (actRange + posibleTarget.Radius); //it use the act range
                if (MapUtilities.PathToPointIsClear(pos, realTargetPoint))
                {
                    reachableTargets.Add(posibleTarget);
                }
            }


            //earlyout
            if (reachableTargets.Count == 0) continue;


            //esto usa el average grupal para lograr lo que desea.
            //Se hace esto para repartir de cierta manera los targets más variadamente y no todos a uno
            var distanceRelativeToAverage = groupAveragePosition - pos;

            var closestTarget = reachableTargets[0];
            var closesetTargetDistance = closestTarget.TargetPosition.Distance(pos + distanceRelativeToAverage);
            for (int i = 1; i < reachableTargets.Count; i++)
            {
                var currentTarget = reachableTargets[i];
                var currDistance = currentTarget.TargetPosition.Distance(pos + distanceRelativeToAverage);
                if (currDistance < closesetTargetDistance)
                {
                    closesetTargetDistance = currDistance;
                    closestTarget = currentTarget;
                }
            }


            //esto ya usa la vision global edl equipo, asi que puede agregar al más cercano.
            tempTargets.Add(member, closestTarget);
        }
    }
}