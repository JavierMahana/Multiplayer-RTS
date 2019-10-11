using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Message : IComponentData
{
    public int TurnToExecute;
    public MessageType Type;    
}
public enum MessageType 
{
    COMMAND_SELF,
    COMMAND_OTHER,
    CONFIRMATION
}
