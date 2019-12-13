using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Javier.RTS;
using UnityEngine;


[DisableAutoCreation]
public class SelectionSystem : ComponentSystem
{
    //public SelectionMarker selectionMarker = GameObject.FindObjectOfType<SelectionMarker>();
    public static SelectedData CurrentSelection { get; private set; } = null;   
    protected override void OnCreate()
    {
        GetEntityQuery(typeof(Simulate));
    }

    private void Deselct()
    {
        CurrentSelection = null;
    }
    private void Select(Entity entity)//, Selectable data)
    {
        CurrentSelection = new SelectedData(entity);
    }
    private bool SelectIfSelectable(Entity entity)
    {
        if (EntityManager.HasComponent<Selectable>(entity))
        {
            //Debug.Log($"Selecting entity of index: {entity.Index}");

            //var selectableData = EntityManager.GetComponentData<Selectable>(entity);
            Select(entity);//, selectableData);
            return true;
        }
        return false;
    }
    protected override void OnUpdate()
    {
        ActiveMap map = MapManager.ActiveMap;
        if (map == null)
        {
            Debug.LogWarning("Create a map before the use of the selection system");
            return;
        }
        //select ->
        if (Input.GetMouseButtonUp(0))
        {
            var mouseHexPos = map.layout.PixelToFractionaHex(Input.mousePosition, Camera.main);
            if (CollisionSystem.PointCast(mouseHexPos, out Entity colliderEntity))
            {
                if (EntityManager.HasComponent<Parent>(colliderEntity))
                {
                    var parent = EntityManager.GetSharedComponentData<Parent>(colliderEntity).ParentEntity;
                    if (SelectIfSelectable(parent))
                    {
                    }
                    else
                    {
                        SelectIfSelectable(colliderEntity);
                    }
                }
                else
                {
                    SelectIfSelectable(colliderEntity);
                }
            }
            else
            {
                Deselct();
            }
        }

        //on selected death ->
        Entities.WithAll<SelectionSystemStateData>().WithNone<Selectable>().ForEach((Entity entity) => 
        {
            PostUpdateCommands.RemoveComponent<SelectionSystemStateData>(entity);
        });
    }
}