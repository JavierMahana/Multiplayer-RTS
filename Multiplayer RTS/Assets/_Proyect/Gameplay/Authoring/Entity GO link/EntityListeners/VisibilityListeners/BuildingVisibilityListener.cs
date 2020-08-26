using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Entities;

public class BuildingVisibilityListener : VisibilityListener
{
    public bool spawnSubstitute = true;
    private bool seenByPlayer = false;

    protected override void OnSight()
    {
        base.OnSight();
        seenByPlayer = true;
    }
    protected override void OutOfSight()
    {
        base.OutOfSight();
        if(seenByPlayer && spawnSubstitute)
        {
            SpawnSubstitute();
        }
        seenByPlayer = false;
    }

    private void SpawnSubstitute()
    {
        var substGO = new GameObject($"{name} substitute", typeof(EntityFilter), typeof(SubstituteVisibilityListener), typeof(PositionListener), typeof(SpriteRenderer));
        substGO.transform.position = transform.position;

        var substEntityFilter = substGO.GetComponent<EntityFilter>();
        var entityManager = entityFilter.EntityManager;

        if (! entityManager.HasComponent<Building>(entityFilter.Entity))
        {
            Debug.LogError("This component requires that the entity has a building component atached!");
            return;
        }

        var buildingComp = entityManager.GetComponentData<Building>(entityFilter.Entity);
        var substEntity = entityManager.CreateEntity(typeof(Substitute), typeof(ExcludeFromSimulation));
        entityManager.AddComponentData<Building>(substEntity, buildingComp);

        substEntityFilter.Initialize(substEntity, entityManager);

        var substSpRenderer = substGO.GetComponent<SpriteRenderer>();
        substSpRenderer.sprite = spRenderer.sprite;
    }
}
