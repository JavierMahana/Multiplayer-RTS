using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System;

[Serializable]
public struct LockstepTurnSystemState : ISystemStateComponentData { }



[UpdateAfter(typeof(LockstepLockSystem))]
public class LockstepTurnFinisherSystem : ComponentSystem
{
    public static int LockstepTurnCounter = 0;

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ExecuteLockstepTurnLogicFlag execute) => 
        {
            LockstepTurnCounter++;
            EntityManager.DestroyEntity(entity);
        });



        Entities.WithNone<LockstepTurnSystemState>().WithAll<Simulate>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<LockstepSystemState>(entity);
        });
        Entities.WithNone<Simulate>().WithAll<LockstepTurnSystemState>().ForEach((Entity entity) =>
        {
            ResetState();
            EntityManager.RemoveComponent<LockstepTurnSystemState>(entity);
        });
    }
    #region Private Methods  
    private void ResetState()
    {
        LockstepTurnCounter = 0;
    }
    #endregion
}