using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
public struct GroupAI : IComponentData
{
    public bool ArePossibleTargets;

    //not implemented
    public bool HavePriorityTarget;
    public Entity PriorityTarget;
}
