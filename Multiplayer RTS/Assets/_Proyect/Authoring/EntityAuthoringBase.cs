using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(EntityFilter))]
public abstract class EntityAuthoringBase : MonoBehaviour
{
    /// <summary>
    /// runs in awake
    /// </summary>
    protected abstract void SetEntityComponents(Entity entity, EntityManager entityManager);
    protected virtual void Awake()
    {
        EntityFilter entityFilter = GetComponent<EntityFilter>();
        Debug.Assert(entityFilter != null, "to use a entity authoring you need to have entity filter on the GO!");

        var entityManager = World.Active.EntityManager;
        var entity = entityManager.CreateEntity();
        SetEntityComponents(entity, entityManager);
        entityFilter.Initialize(entity, entityManager);
    }

}
