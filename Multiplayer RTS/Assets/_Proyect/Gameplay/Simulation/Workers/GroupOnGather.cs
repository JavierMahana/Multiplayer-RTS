using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
//actualmente este componente es agregado y eliminado unicamente por commandos
public struct GroupOnGather : IComponentData
{
    //tag comp when the player wants the group to be on the gathering state.
    public ResourceType GatheringResourceType;
}
