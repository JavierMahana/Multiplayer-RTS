using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DropOfBuildingAuthoringComponent : BuildingAuthoringComponent
{
    public bool canDropFood;
    public bool canDropWood;
    public bool canDropGold;
    public bool canDropStone;
    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        base.SetEntityComponents(entity, entityManager);

        entityManager.AddComponentData<ResourceDropPoint>(entity, new ResourceDropPoint()
        { CanDropFood = canDropFood, CanDropWood = canDropWood, CanDropGold = canDropGold, CanDropStone = canDropStone });
    }
}
