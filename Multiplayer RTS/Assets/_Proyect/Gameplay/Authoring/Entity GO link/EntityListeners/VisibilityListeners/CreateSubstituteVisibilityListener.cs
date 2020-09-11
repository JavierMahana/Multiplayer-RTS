using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSubstituteVisibilityListener : VisibilityListener
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
        if (seenByPlayer && spawnSubstitute)
        {
            SpawnSubstitute();
        }
        seenByPlayer = false;
    }

    private void SpawnSubstitute()
    {
        var entityManager = entityFilter.EntityManager;
        Hex hex;
        if (entityManager.HasComponent<Building>(entityFilter.Entity))
        {
            var buildingComp = entityManager.GetComponentData<Building>(entityFilter.Entity);
            hex = buildingComp.position;
        }
        else if (entityManager.HasComponent<ResourceSource>(entityFilter.Entity))
        {
            var resourceComp = entityManager.GetComponentData<ResourceSource>(entityFilter.Entity);
            hex = resourceComp.position;
        }
        else
        {
            Debug.Log("To create a substitute you need that the entity that creates it is a building or a resource! Not creating a substitute.");
            return;
        }

        var substGO = new GameObject($"{name} substitute", typeof(EntityFilter), typeof(SubstituteVisibilityListener), typeof(PositionListener), typeof(SpriteRenderer));
        substGO.transform.position = transform.position;

        var substEntityFilter = substGO.GetComponent<EntityFilter>();
        



        var substEntity = entityManager.CreateEntity(typeof(Substitute), typeof(ExcludeFromSimulation));
        entityManager.SetComponentData<Substitute>(substEntity, new Substitute() { position = hex });

        substEntityFilter.Initialize(substEntity, entityManager);

        var substSpRenderer = substGO.GetComponent<SpriteRenderer>();
        substSpRenderer.sprite = spRenderer.sprite;
    }
}
