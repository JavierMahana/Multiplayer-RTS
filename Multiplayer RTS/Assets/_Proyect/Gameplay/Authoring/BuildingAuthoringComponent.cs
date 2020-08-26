using FixMath.NET;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor.XR;
using UnityEngine;

public class BuildingAuthoringComponent : EntityAuthoringBase
{

    public int team = 0;
    public float sightRange = 1;
    public bool blockMovement = true;
    protected override void SetEntityComponents(Entity entity, EntityManager entityManager)
    {
        if (MapManager.ActiveMap == null)
        {
            var mapManager = FindObjectOfType<MapManager>();
            Debug.Assert(mapManager != null, "You must have an Map Manager object in the scene in order to be able to author units from the editor!", this);
            mapManager.LoadMap(mapManager.mapToLoad);
        }
        Layout layout = MapManager.ActiveMap.layout;
        var fractionalHex = layout.WorldToFractionalHex(new FixVector2((Fix64)transform.position.x, (Fix64)transform.position.y));

        Hex hex = fractionalHex.Round();

        entityManager.AddComponentData<Building>(entity, new Building() { position = hex });

        if (blockMovement)
            entityManager.AddComponentData<BlockMovement>(entity, new BlockMovement() { position = hex });

        entityManager.AddComponentData<Team>(entity, new Team() { Number = team });

        entityManager.AddComponentData<SightRange>(entity, new SightRange() { Value = (Fix64)sightRange });

    }

}
