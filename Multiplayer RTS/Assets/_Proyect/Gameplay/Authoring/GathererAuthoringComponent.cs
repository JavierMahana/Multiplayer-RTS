using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
[RequireComponent(typeof(EntityFilter))]
public class GathererAuthoringComponent : BaseUnitAuthoringComponent
{

    [Title("Gather attributes")]
    [SerializeField]
    private int maxCargo = 10;
    [SerializeField]
    private int woodChopingSpeed = 2;
    [SerializeField]
    private int farmingSpeed = 2;
    [SerializeField]
    private int goldMiningSpeed = 1;
    [SerializeField]
    private int stoneMiningSpeed = 1;

    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        base.SetEntityComponents(entity, entityManager);

        entityManager.AddComponentData<Gatherer>(entity, new Gatherer()
        {
            maxCargo = maxCargo,
            woodChopingSpeed = woodChopingSpeed,
            farmingSpeed = farmingSpeed,
            goldMiningSpeed = goldMiningSpeed,
            stoneMiningSpeed = stoneMiningSpeed
        });
    }


}
