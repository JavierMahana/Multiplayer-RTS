using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AssigningUnitParentAuthoring : MonoBehaviour
{
    private bool unitCallBackRecieved = false;
    private bool parentCallBackRecieved = false;

    private List<Entity> unitEntities = new List<Entity>();
    //private Entity unitEntity;
    private Entity parentEntity;

    public void ParentEntityCreatedCallback(Entity parent)
    {
        parentEntity = parent;
        parentCallBackRecieved = true;
    }
    public void UnitEntityCreatedCallback(Entity unit)
    {
        unitEntities.Add(unit);
        unitCallBackRecieved = true;
    }

    void Start()
    {
        if (unitCallBackRecieved && parentCallBackRecieved)
        {
            foreach (var unit in unitEntities)
            {
                World.Active.EntityManager.SetSharedComponentData<Parent>(unit, new Parent() { ParentEntity = parentEntity });
            }
        }
        else 
        {
            Debug.LogWarning($"you don't recieved any or both of the callbacks from the unit and parent. Unit callback recieved: {unitCallBackRecieved}, Parent callbakc recieved: {parentCallBackRecieved}");
        }
    }


}
