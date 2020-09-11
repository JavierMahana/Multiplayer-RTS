using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


//"AutomaticActBehaviour" me da paja cambiarle el nombre xd
[Serializable]
public struct ActTargetFilters : IComponentData
{
    public bool ActOnTeamates;
    public bool ActOnEnemies;

    public ActType actType;
}
