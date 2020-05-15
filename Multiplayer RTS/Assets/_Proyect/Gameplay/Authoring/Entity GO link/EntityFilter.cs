using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EntityFilter : MonoBehaviour
{
    public Entity Entity { get; private set; }
    public EntityManager EntityManager { get; private set; }

    public void Initialize(Entity entity, EntityManager entityManager)
    {
        Entity = entity;
        EntityManager = entityManager;
    }

}
