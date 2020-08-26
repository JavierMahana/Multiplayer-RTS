using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GathererGroupAuthoringComponent : BaseGroupAuthoringComponent
{
    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        base.SetEntityComponents(entity, entityManager);

        entityManager.AddBuffer<BEResourceSource>(entity);
        entityManager.AddComponent<HasGatherer>(entity);

        entityManager.SetComponentData<Commandable>(entity, new Commandable() { DeafaultCommand = CommandType.GATHER_COMMAND });
    }
}
