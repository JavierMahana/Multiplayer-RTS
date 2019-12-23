using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
public struct ActTargetFilters : IComponentData
{
    public bool ActOnTeamates;
    public bool ActOnEnemies;
}
