using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubstituteVisibilityListener : VisibilityListener
{
    protected override void OnSight()
    {
        entityFilter.EntityManager.DestroyEntity(entityFilter.Entity);
        Destroy(this.gameObject);
        
    }
    protected override void OutOfSight()
    {

    }
}
