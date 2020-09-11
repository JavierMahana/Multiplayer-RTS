using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using UnityEngine;
using FixMath.NET;

[DisableAutoCreation]
//it creates pathfindig triggers in the following cases:
//1- the entity have a refresh path now component
//2- the entity have a path refresh state component and enough game turns have passed
//depends on hte system OnGroupCheck
//it have the a path refresh system state to work properly; if it dont have it, then it creates it. if it have it but don't have
//a "OnReinforncement" or "Group" tags it eliminates it.

    //uses PostUpdateCommands as the pathfindig system depends on the result of these calculations
    //this system is not working for group especial movemen as they require to set a destination point based on more complex parameters
public class PathRefreshSystem : ComponentSystem
{

    private void TriggerPathFindingToParent(Entity entity, Parent parent, ref RefreshPathTimer refreshPathTimer)
    {
        if (!EntityManager.HasComponent<HexPosition>(parent.ParentEntity))
        {
            Debug.LogError("the parent must have a position component in order to allow this system to work properly");
            return;
        }
        var parentPosition = EntityManager.GetComponentData<HexPosition>(parent.ParentEntity);

        PostUpdateCommands.AddComponent(entity, new TriggerPathfinding() { Destination = parentPosition.HexCoordinates.Round() });
        refreshPathTimer.TurnsWithoutRefresh = 0;
    }
    private void TriggerPathFindingOnUnitWithTarget(Entity entity, FractionalHex pos, ActionTarget target, RuntimeMap map, ref RefreshPathTimer refreshPathTimer)
    {
        Hex dest;
        MapUtilities.TryFindClosestOpenAndReachableHex(out dest, (FractionalHex)target.OccupyingHex, pos, map.MovementMapValues);
        PostUpdateCommands.AddComponent(entity, new TriggerPathfinding() { Destination = dest });
        refreshPathTimer.TurnsWithoutRefresh = 0;
    }

    private void TriggerPathFindingOnCommandedGroup(Entity entity, Hex destinationHex, ref RefreshPathTimer refreshPathTimer)
    {        
        PostUpdateCommands.AddComponent(entity, new TriggerPathfinding() { Destination = destinationHex });
        refreshPathTimer.TurnsWithoutRefresh = 0;
    }


    protected override void OnUpdate()
    {
        var map = MapManager.ActiveMap;
        Debug.Assert(map != null, "the active map must be set before this systems updates");


        #region refresh pathnow
        #region group variants
        Entities.WithAll<RefreshPathNow, Group>().WithNone<PriorityGroupTarget>().ForEach(
        (Entity entity, ref DestinationHex destination, ref RefreshPathTimer timer) =>
        {
            Debug.Log("instant refresh called!");
            TriggerPathFindingOnCommandedGroup(entity, destination.FinalDestination, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
        });

        Entities.WithAll<RefreshPathNow, Group>().ForEach(
        (Entity entity, ref PriorityGroupTarget target, ref RefreshPathTimer timer) =>
        {
            Debug.Log("instant refresh called!");
            TriggerPathFindingOnCommandedGroup(entity, target.TargetHex, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
        });


        #endregion

        //,OnReinforcement
        Entities.WithAll<RefreshPathNow>().WithNone<ActionTarget>().ForEach(
        (Entity entity, Parent parent, ref RefreshPathTimer timer) => 
        {            
            TriggerPathFindingToParent(entity, parent, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
        });


        Entities.WithAll<RefreshPathNow, OnGroup>().ForEach(
        (Entity entity, ref HexPosition pos, ref ActionTarget target, ref RefreshPathTimer timer) =>
        {
            TriggerPathFindingOnUnitWithTarget(entity, pos.HexCoordinates, target, map.map, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);

            //if (!MapUtilities.PathToPointIsClear(pos.HexCoordinates, target.TargetPosition))
            //{
            //    TriggerPathFindingOnUnitWithTarget(entity, pos.HexCoordinates, target, map.map, ref timer);
            //    PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
            //}

        });

        #endregion



        #region automatic refresh
        //,OnReinforcement
        Entities.WithAll<PathRefreshSystemState>().WithNone<ActionTarget>().ForEach(
        (Entity entity, Parent parent, ref RefreshPathTimer refreshPathTimer) => 
        {
            if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            {
                TriggerPathFindingToParent(entity, parent, ref refreshPathTimer);
            }
            else
            {
                refreshPathTimer.TurnsWithoutRefresh += 1;
            }
        });

        Entities.WithAll<OnGroup, PathRefreshSystemState>().ForEach(
        (Entity entity, ref HexPosition pos, ref ActionTarget target, ref RefreshPathTimer refreshPathTimer) =>
        {
            if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            {
                TriggerPathFindingOnUnitWithTarget(entity, pos.HexCoordinates, target, map.map, ref refreshPathTimer);
            }
            else
            {
                refreshPathTimer.TurnsWithoutRefresh += 1;
            }
            //if (!MapUtilities.PathToPointIsClear(pos.HexCoordinates, target.TargetPosition))
            //{
            //    if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            //    {
            //        TriggerPathFindingOnUnitWithTarget(entity, pos.HexCoordinates, target, map.map, ref refreshPathTimer);
            //    }
            //    else
            //    {
            //        refreshPathTimer.TurnsWithoutRefresh += 1;
            //    }
            //}
        });


        #region group variants
        Entities.WithAll<Group, PathRefreshSystemState>().WithNone<PriorityGroupTarget>().ForEach(
        (Entity entity, ref DestinationHex destination, ref RefreshPathTimer refreshPathTimer) =>
        {            
            if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            {
                TriggerPathFindingOnCommandedGroup(entity, destination.FinalDestination, ref refreshPathTimer);
            }
            else
            {
                refreshPathTimer.TurnsWithoutRefresh += 1;
            }
        });

        Entities.WithAll<Group, PathRefreshSystemState>().ForEach(
        (Entity entity, ref PriorityGroupTarget target, ref RefreshPathTimer refreshPathTimer) =>
        {
            if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            {
                TriggerPathFindingOnCommandedGroup(entity, target.TargetHex, ref refreshPathTimer);
            }
            else
            {
                refreshPathTimer.TurnsWithoutRefresh += 1;
            }
        });
        #endregion
        #endregion



        #region refresh counter managment
        Entities.WithAll<RefreshPathTimer>().WithNone<PathRefreshSystemState>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.AddComponent<PathRefreshSystemState>(entity);
        });

        Entities.WithAll<PathRefreshSystemState>().WithNone<RefreshPathTimer>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.RemoveComponent<PathRefreshSystemState>(entity);
        });
        #endregion
    }
}