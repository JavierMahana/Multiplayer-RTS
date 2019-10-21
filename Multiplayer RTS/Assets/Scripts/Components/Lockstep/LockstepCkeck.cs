using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LockstepCkeck : IComponentData
{
    public int Turn;
    public LockstepCheckType Type;
}
public enum LockstepCheckType
{
    COMMAND,
    CONFIRMATION
}
