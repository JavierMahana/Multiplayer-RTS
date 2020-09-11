using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;



public class EntityFilter : MonoBehaviour
{
    //una entidad se considera destruida cuando no tiene EntityFilterEntityAlive y si tiene EntityFilterPostDeathRemanent
    //public bool destroyObjectIfEntityIsDestroyed = true;

    public Entity Entity { get; private set; }
    public EntityManager EntityManager { get; private set; }
    //public bool Alive { get; private set; } = true;
    private bool HaveBeenInited = false;

    public void Initialize(Entity entity, EntityManager entityManager)
    {
        Entity = entity;
        EntityManager = entityManager;

        HaveBeenInited = true;
        entityManager.AddComponent<EntityFilterEntityAlive>(entity);
        entityManager.AddComponent<EntityFilterPostDeathRemanent>(entity);
    }

    private void Update()
    {
        if (HaveBeenInited)
        {
            if (EntityManager.HasComponent<EntityFilterPostDeathRemanent>(Entity) && (!EntityManager.HasComponent<EntityFilterEntityAlive>(Entity)))
            {                
                EntityManager.RemoveComponent<EntityFilterPostDeathRemanent>(Entity);
                Destroy(gameObject);
            }
        }
    }
}
