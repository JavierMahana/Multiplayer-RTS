using Unity.Entities;


public class EntityOnVision
{
    public EntityOnVision(Entity entity, Collider collider, Team team, FractionalHex position)
    {
        this.entity = entity;
        this.collider = collider;
        this.team = team.Number;
        this.position = position;
    }
    public EntityOnVision(Entity entity, Collider collider, int team, FractionalHex position)
    {
        this.entity = entity;
        this.collider = collider;
        this.team = team;
        this.position = position;
    }
    public readonly Entity entity;
    public readonly Collider collider;
    public readonly int team;
    public readonly FractionalHex position;
}
