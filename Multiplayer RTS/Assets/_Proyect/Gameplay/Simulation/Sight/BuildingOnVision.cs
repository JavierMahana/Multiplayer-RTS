using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BuildingOnVision 
{
    public BuildingOnVision(Entity entity, Building building, Team team)
    {
        this.entity = entity;
        this.building = building;
        this.team = team.Number;
    }
    public BuildingOnVision(Entity entity, Building building, int team)
    {
        this.entity = entity;
        this.building = building;
        this.team = team;
    }
    public readonly Entity entity;
    public readonly Building building;
    public readonly int team;
}
