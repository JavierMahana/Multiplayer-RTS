using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(Renderer))]
public class SelectionMarker : MonoBehaviour
{
    private new Renderer renderer;
    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }
    void Update()
    {        
        var currentSelection = SelectionSystem.CurrentSelection;
        if (currentSelection != null)
        {
            var entityManager = World.Active.EntityManager;
            if (entityManager.HasComponent<HexPosition>(currentSelection.entity))
            {
                renderer.enabled = true;

                var position = entityManager.GetComponentData<HexPosition>(currentSelection.entity).HexCoordinates;
                var activeMap = MapManager.ActiveMap;
                if (activeMap != null)
                {
                    var worldPos = activeMap.layout.HexToWorld(position);
                    transform.position = new Vector3((float)worldPos.x, (float)worldPos.y, (float)HexTranslationSystem.AGENTS_Z_VALUE + 0.5f);
                }
            }
        }
        else
        {
            renderer.enabled = false;            
        }
    }
}
