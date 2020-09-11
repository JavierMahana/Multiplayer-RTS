using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct IsActing : IComponentData
{
    //USED IN THE ACTION SYSTEM.
    public ActType ActType;
    public Entity ActingEntity;
}
