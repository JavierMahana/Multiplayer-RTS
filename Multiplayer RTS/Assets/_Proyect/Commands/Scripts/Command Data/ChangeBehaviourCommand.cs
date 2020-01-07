
using Unity.Entities;

public struct ChangeBehaviourCommand 
{
    public Entity Target;
    public GroupBehaviour NewBehaviour;
}
