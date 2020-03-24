using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TestAuthoring : EntityAuthoringBase
{
    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        entityManager.AddComponentData<HexPosition>(entity, new HexPosition() { HexCoordinates = new FractionalHex((FixMath.NET.Fix64)5, (FixMath.NET.Fix64)5) });
    }
}
