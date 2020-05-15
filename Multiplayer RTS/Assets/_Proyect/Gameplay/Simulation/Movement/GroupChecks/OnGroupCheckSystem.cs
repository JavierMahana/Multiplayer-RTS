using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

//it uses the post update commands.
//because it is supposed to run after all of the other simulation systems.

[DisableAutoCreation]
//this systems checks if a entity is consider inside a his parent group
//situations where the entity is outside the group
//1- if the entity is too away from his parent 
//2- there are unwalkable hexes in bewtween 
//3- there are on different heights -> these must consider the ramps 
//this is the first system in the movement system

public class OnGroupCheckSystem : ComponentSystem
{
    private static readonly Fix64 OnGroupThresholdDistance = (Fix64)2;
    private bool EntityIsOnGroup(int entityIndex, HexPosition position, HexPosition parentPosition, RuntimeMap map) 
    {
        var parentCoords = parentPosition.HexCoordinates;
        var entityCoords = position.HexCoordinates;

        var distance = parentCoords.Distance(entityCoords) ;
        if (distance > OnGroupThresholdDistance)
        {
            return false;
        }
        return MapUtilities.PathToPointIsClear(entityCoords, parentCoords);
    }
    protected override void OnUpdate()
    {
        //ComponentDataFromEntity<HexPosition> positionFromEntity = GetComponentDataFromEntity<HexPosition>(true);
        var activeMap = MapManager.ActiveMap;
        if (activeMap == null)
        {
            Debug.LogError("the group check system needs an active world to function!");
            return;
        }

        Entities.WithAll<OnGroup>().ForEach((Entity entity, Parent parent, ref HexPosition position) => 
        {
            if (!EntityManager.HasComponent<HexPosition>(parent.ParentEntity)) 
            {
                Debug.LogError("the parent must have a position component in order to allow this system to work properly");
                return;
            }
            var parentPosition = EntityManager.GetComponentData<HexPosition>(parent.ParentEntity);

            if (! EntityIsOnGroup(entity.Index, position, parentPosition, activeMap.map))
            {
                PostUpdateCommands.AddComponent<OnReinforcement>(entity);
                PostUpdateCommands.AddComponent<RefreshPathNow>(entity);                

                PostUpdateCommands.RemoveComponent<OnGroup>(entity);
            }
        });
        Entities.WithAll<OnReinforcement>().ForEach((Entity entity, Parent parent, ref HexPosition position) =>
        {
            if (!EntityManager.HasComponent<HexPosition>(parent.ParentEntity))
            {
                return;
            }
            var parentPosition = EntityManager.GetComponentData<HexPosition>(parent.ParentEntity);
            if (EntityIsOnGroup(entity.Index, position, parentPosition, activeMap.map))
            {
                PostUpdateCommands.RemoveComponent<OnReinforcement>(entity);
                PostUpdateCommands.AddComponent<OnGroup>(entity);                
            }
        });

    }
}