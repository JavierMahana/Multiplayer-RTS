using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


/// <summary>
/// this class is temporary out of use because we are testing the renderer
/// </summary>
[RequireComponent(typeof(Renderer))]
public class SelectionMarker : MonoBehaviour
{
    private new Renderer renderer;
    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        //this must be a sprite renderer.
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
                    transform.position = new Vector3((float)worldPos.x, (float)worldPos.y);
                }
            }
        }
        else
        {
            renderer.enabled = false;            
        }
    }
}
