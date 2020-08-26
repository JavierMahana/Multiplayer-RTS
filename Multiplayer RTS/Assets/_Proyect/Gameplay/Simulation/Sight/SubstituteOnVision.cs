using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SubstituteOnVision 
{
    public SubstituteOnVision(Entity entity, Building building)
    {
        this.entity = entity;
        this.building = building;
    }
    public Building building;
    public Entity entity;
}
