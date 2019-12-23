using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;
using UnityEngine;

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

    private void TriggerPathFindingOnReinforcementUnit(Entity entity, Parent parent, ref RefreshPathTimer refreshPathTimer)
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
    private void TriggerPathFindingOnCommandedGroup(Entity entity, Hex destinationHex, ref RefreshPathTimer refreshPathTimer)
    {       
        PostUpdateCommands.AddComponent(entity, new TriggerPathfinding() { Destination = destinationHex });
        refreshPathTimer.TurnsWithoutRefresh = 0;
    }


    protected override void OnUpdate()
    {

        #region refresh pathnow
        Entities.WithAll<RefreshPathNow, Group>().ForEach(
        (Entity entity, ref DestinationHex destination, ref RefreshPathTimer timer) =>
        {   
            TriggerPathFindingOnCommandedGroup(entity, destination.FinalDestination, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
        });

        Entities.WithAll<RefreshPathNow, OnReinforcement>().ForEach(
        (Entity entity, Parent parent, ref RefreshPathTimer timer) => 
        {            
            TriggerPathFindingOnReinforcementUnit(entity, parent, ref timer);
            PostUpdateCommands.RemoveComponent<RefreshPathNow>(entity);
        });
        #endregion



        #region automatic refresh
        Entities.WithAll<OnReinforcement, PathRefreshSystemState>().ForEach(
        (Entity entity, Parent parent, ref RefreshPathTimer refreshPathTimer) => 
        {
            if (refreshPathTimer.TurnsRequired <= refreshPathTimer.TurnsWithoutRefresh)
            {
                TriggerPathFindingOnReinforcementUnit(entity, parent, ref refreshPathTimer);
            }
            else
            {
                refreshPathTimer.TurnsWithoutRefresh += 1;
            }
        });

        Entities.WithAll<Group, PathRefreshSystemState>().ForEach(
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
        #endregion



        #region refresh counter managment
        Entities.WithAll<OnReinforcement, RefreshPathTimer>().WithNone<PathRefreshSystemState>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.AddComponent<PathRefreshSystemState>(entity);
        });


        Entities.WithAll<PathRefreshSystemState>().WithNone<Group, OnReinforcement>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.RemoveComponent<PathRefreshSystemState>(entity);
        });
        #endregion
    }
}