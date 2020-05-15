using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SelectedData
{
    public Entity entity;
    public SelectedData(Entity entity)
    {
        this.entity = entity;
    }
}
