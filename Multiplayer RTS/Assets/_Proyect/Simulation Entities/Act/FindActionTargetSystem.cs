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
[DisableAutoCreation]
public class FindActionTargetSystem : ComponentSystem
{
    private EntityQuery m_GroupQuery;
    private EntityQuery m_OnGroupQuery;    

    protected override void OnCreate()
    {
        m_GroupQuery = GetEntityQuery(typeof(Group), typeof(GroupAI), typeof(BEPosibleTarget));
        m_OnGroupQuery = GetEntityQuery(typeof(OnGroup), typeof(Parent), typeof(HexPosition));        
    }



    protected override void OnUpdate()
    {
        int groupCount    = m_GroupQuery.CalculateEntityCount();        
        var groupTriggers = new bool[groupCount];
        var groupIndices  = new Dictionary<int, int>(groupCount);
        var groupMemberPostionSum = new FractionalHex[groupCount];        


        int onGroupCount         = m_OnGroupQuery.CalculateEntityCount();        
        var onGroupEntityToIndex = new Dictionary<int, int>(onGroupCount);
        var onGroupPositions     = new FractionalHex[onGroupCount];
        var tempTargets          = new Dictionary<int, ActionTarget>();

        var groupMemberHashMap = new Dictionary<int, List<int>>(onGroupCount);
        var onGroupParents = new Entity[onGroupCount];

        

        int groupIterator = 0;
        Entities.WithAll<Group, BEPosibleTarget>().ForEach((Entity entity, ref GroupAI groupAIState) =>
        {                                                           
            groupTriggers[groupIterator] = groupAIState.ArePossibleTargets;
            groupIndices.Add(entity.Index, groupIterator);
            groupIterator++;
        });

        int onGroupIterator = 0;
        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref HexPosition hexPosition) =>
        {
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
            
            bool parentIsTriggered = groupTriggers[parentIndex];

            if (parentIsTriggered)
            {                
                var parentTargetsBuffer = EntityManager.GetBuffer<BEPosibleTarget>(parentEntity);
                Debug.Assert(parentTargetsBuffer.IsCreated && parentTargetsBuffer.Length > 0, $"The target buffer of enity:{parentEntityIndex} has a problem. iscreated:{parentTargetsBuffer.IsCreated}. lenght:{parentTargetsBuffer.Length}");                
                foreach (int member in groupMemberList)
                {
                    //esto usa el average grupal para lograr lo que desea.
                    //Se hace esto para repartir de cierta manera los targets más variadamente y no todos a uno
                    
                    var pos = onGroupPositions[member];
                    var distanceRelativeToAverage = groupAveragePosition - pos;

                    var closestTarget = parentTargetsBuffer[0];
                    var closesetTargetDistance = closestTarget.Position.Distance(pos + distanceRelativeToAverage);
                    for (int i = 1; i < parentTargetsBuffer.Length; i++)
                    {
                        var currentTarget = parentTargetsBuffer[i];
                        var currDistance = currentTarget.Position.Distance(pos + distanceRelativeToAverage);
                        if (currDistance < closesetTargetDistance)
                        {
                            closesetTargetDistance = currDistance;
                            closestTarget = currentTarget;
                        }
                    }

                    //if(pasive){ //if(distance <= actDistance) -> set target}

                    tempTargets.Add(member, new ActionTarget()
                    {
                        TargetEntity = closestTarget.Entity,
                        TargetPosition = closestTarget.Position,
                        TargetRadius = closestTarget.Radius
                    });
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
}