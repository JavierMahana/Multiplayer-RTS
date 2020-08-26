using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Entities;

//this class listens if the entity must be visible or not.
//It may be usefull to have a backup sprite, that activates when this entity is not on vision.-> this might be better as another monobehaviour.
[RequireComponent(typeof(EntityFilter))]
public class VisibilityListener : MonoBehaviour
{
    public bool IsVisible => spRenderer == null ? false : spRenderer.enabled;
    protected EntityFilter entityFilter;
    //agregar otros componentes que se quisieran desactivar al dejar de ver la entidad.
    protected SpriteRenderer spRenderer;
    
    private void Awake()
    {
        entityFilter = GetComponent<EntityFilter>();
        spRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var entititiesOnSight = SightSystem.GetEntitiesOnVisionOfTeamHashSet(GameManager.PlayerTeams.ToArray());
        if(entititiesOnSight.Contains(entityFilter.Entity))
        {
            OnSight();
        }
        else
        {
            OutOfSight();
        }
    }

    protected virtual void OnSight()
    {
        spRenderer.enabled = true;
    }
    protected virtual void OutOfSight()
    {
        spRenderer.enabled = false;
    }
}
