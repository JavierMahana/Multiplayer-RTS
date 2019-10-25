using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;



[UpdateBefore(typeof(CommandNetworkSenderSystem))]
public class LockstepLockSystem : ComponentSystem
{
    #region Public Static Fields (todas estas variables despues se podrian transformar en propiedades para tener más seguridad)
    public static bool LockstepActivated = false;
    public const int NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE = 2;
    #endregion
    private bool logg = true;
    private int lastGameTickWithOpenLocktep = int.MinValue;
    private const int TICKS_REQUIRED_FOR_LOCKSTEP_TURN = 4;

    #region Component System CallBacks
    protected override void OnUpdate()
    {
        Entities.WithAll<Simulate, LockstepSystemState>().ForEach((Entity entity) =>
        {
            int currTick = SimulationTickFinisherSystem.TickCounter;
            if (currTick % TICKS_REQUIRED_FOR_LOCKSTEP_TURN == 0 && currTick != lastGameTickWithOpenLocktep)
            {
                if (LockstepCheckSystem.AllCheksOfTurnAreRecieved(LockstepTurnFinisherSystem.LockstepTurnCounter))
                {
                    if (logg) Debug.Log($"passing lock in turn {LockstepTurnFinisherSystem.LockstepTurnCounter}");
                    EntityManager.CreateEntity(typeof(ExecuteLockstepTurnLogicFlag));

                    lastGameTickWithOpenLocktep = SimulationTickFinisherSystem.TickCounter;
                    LockstepActivated = false;                    
                }
                else
                {
                    LockstepActivated = true;
                }
            }
        });




        Entities.WithNone<LockstepSystemState>().WithAll<Simulate>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<LockstepSystemState>(entity);
        });

        Entities.WithNone<Simulate>().WithAll<LockstepSystemState>().ForEach((Entity entity) =>
        {
            ResetState();

            EntityManager.RemoveComponent<LockstepSystemState>(entity);
        });

    }


    #endregion
    #region Private Methods  
    private void ResetState()
    {
        lastGameTickWithOpenLocktep = int.MinValue;
        LockstepActivated = true;
    }
    #endregion
}