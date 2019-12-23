using FixMath.NET;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public struct MainSimulationStateComponent : ISystemStateComponentData { }
public class MainSimulationLoopSystem : ComponentSystem
{
    public static int CurrentLockstepTurn { get; private set; } = 0;
    public static int CurrentGameTurn { get; private set; }     = 0;
    public static Fix64 SimulationTime { get; private set; }    = Fix64.Zero;
    public static Fix64 UnprocessedTime { get; private set; }   = Fix64.Zero;

    public static readonly Fix64 SimulationDeltaTime            = (Fix64)0.05;
    public const int GAME_TURNS_REQUIRED_FOR_LOCKSTEP_TURN      = 4;
    public const int COMMANDS_DELAY                             = 2;


    private bool lockstepIsOpen = false;
    private int lastGameTurnWhereLockstepWasOpen = 0;

    //always updating systems
    private InputSystem inputSystem;
    private SelectionSystem selectionSystem;


    private LockstepSystemGroup lockstepSystemGroup;
    //private SimulationSystemGroup simulationSystemGroup;
    //private LateSimulationSystemGroup lateSimulationSystemGroup;
    //private ApplicationSystemGroup applicationSystemGroup;
    private OnGroupCheckSystem onGroupCheckSystem;
    private PathRefreshSystem pathRefreshSystem;
    private PathFindingSystem pathFindingSystem;
    private PathChangeIndexSystem pathChangeIndexSystem;

    
    private FindPosibleTargetsSystem findPosibleTargetsSystem;
    private FindActionTargetSystem findActionTargetSystem;

    private FindMovementTargetSystem findMovementTargetSystem;
    private SteeringSystem steeringSystem;
    private TranslationSystem translationSystem;
    private MovementFinisherSystem movementFinisherSystem;

    private CollisionSystem collisionSystem;
    private DirectionSystem directionSystem;
    

    protected override void OnCreate()
    {
        inputSystem = World.GetOrCreateSystem<InputSystem>();
        selectionSystem = World.GetOrCreateSystem<SelectionSystem>();

        lockstepSystemGroup = World.GetOrCreateSystem<LockstepSystemGroup>();
        lockstepSystemGroup.AddSystemToUpdateList(World.GetOrCreateSystem<CommandExecutionSystem>());
        lockstepSystemGroup.AddSystemToUpdateList(World.GetOrCreateSystem<VolatileCommandSystem>());
        lockstepSystemGroup.AddSystemToUpdateList(World.GetOrCreateSystem<CommandableSafetySystem>());

        onGroupCheckSystem       = World.GetOrCreateSystem<OnGroupCheckSystem>();
        pathRefreshSystem        = World.GetOrCreateSystem<PathRefreshSystem>();
        pathFindingSystem        = World.GetOrCreateSystem<PathFindingSystem>();
        pathChangeIndexSystem    = World.GetOrCreateSystem<PathChangeIndexSystem>();

        findPosibleTargetsSystem            = World.GetOrCreateSystem<FindPosibleTargetsSystem>();
        findActionTargetSystem   = World.GetOrCreateSystem<FindActionTargetSystem>();

        findMovementTargetSystem = World.GetOrCreateSystem<FindMovementTargetSystem>();
        steeringSystem           = World.GetOrCreateSystem<SteeringSystem>();
        translationSystem        = World.GetOrCreateSystem<TranslationSystem>();
        movementFinisherSystem   = World.GetOrCreateSystem<MovementFinisherSystem>();

        collisionSystem          = World.GetOrCreateSystem<CollisionSystem>();
        directionSystem          = World.GetOrCreateSystem<DirectionSystem>();
        //simulationSystemGroup = World.GetOrCreateSystem<SimulationSystemGroup>();
        //simulationSystemGroup.AddSystemToUpdateList(World.GetOrCreateSystem<PathFindingSystem>());
        //simulationSystemGroup.AddSystemToUpdateList(World.GetOrCreateSystem<PathFollowSystem>());

        //lateSimulationSystemGroup = World.GetOrCreateSystem<LateSimulationSystemGroup>();

        //applicationSystemGroup = World.GetOrCreateSystem<ApplicationSystemGroup>();
    }
    protected override void OnUpdate()
    {

        Entities.WithAll<Simulate, MainSimulationStateComponent>().ForEach((Entity entity) => 
        {
            inputSystem.Update();
            selectionSystem.Update();


            if (CurrentGameTurn % GAME_TURNS_REQUIRED_FOR_LOCKSTEP_TURN == 0 && CurrentGameTurn != lastGameTurnWhereLockstepWasOpen || !lockstepIsOpen)
            {
                
                lockstepIsOpen = LockstepCheckSystem.AllCheksOfTurnAreRecieved(CurrentLockstepTurn);                
                if (lockstepIsOpen)
                {
                    lockstepSystemGroup.Update();

                    lastGameTurnWhereLockstepWasOpen = CurrentGameTurn;
                    CurrentLockstepTurn++;
                }
            }
            if (!lockstepIsOpen)
                return;


            Fix64 deltaTime = (Fix64)Time.deltaTime;
            SimulationTime += deltaTime;
            UnprocessedTime += deltaTime;

            if (SimulationDeltaTime <= UnprocessedTime)
            {
                //var entitis = World.EntityManager.GetAllEntities();
                //foreach (var entiti in entitis)
                //{
                //    if (entiti.Index == 101) 
                //    {
                //        var pos = EntityManager.GetComponentData<HexPosition>(entiti);
                //        var des = EntityManager.GetComponentData<DesiredMovement>(entiti);
                //        Debug.Log($"entity 101: positon: {pos.HexCoordinates}. rounded position: {pos.HexCoordinates.Round()}. The desiredMov: {des.Value}");
                //    }
                //}
                //entitis.Dispose();

                onGroupCheckSystem.Update();
                pathRefreshSystem.Update();
                pathFindingSystem.Update();
                pathChangeIndexSystem.Update();

                findPosibleTargetsSystem.Update();
                findActionTargetSystem.Update();

                findMovementTargetSystem.Update();
                steeringSystem.Update();
                translationSystem.Update();
                movementFinisherSystem.Update();

                directionSystem.Update();

                //collisions systems
                collisionSystem.Update();

                
                //simulationSystemGroup.Update();
                //lateSimulationSystemGroup.Update();
                //applicationSystemGroup.Update();
                UnprocessedTime -= SimulationDeltaTime;
                CurrentGameTurn++;
            }
        });

        Entities.WithAll<Simulate>().WithNone<MainSimulationStateComponent>().ForEach((Entity entity) => 
        {
            EntityManager.AddComponent<MainSimulationStateComponent>(entity);
        });
        Entities.WithNone<Simulate>().WithAll<MainSimulationStateComponent>().ForEach((Entity entity) => 
        {
            ResetState();
            EntityManager.RemoveComponent<MainSimulationStateComponent>(entity);
        });
        // have elapsed enought game ticks for another lockstep check and
        // the last turn that the the lockstep had been checked is not the current one or the lockstep is closed?
        //     yes)assignate if the lockstep is open or closed
        // 
        // the lockstep is open?
        //  yes) execute lockstep system group
        //  no)  return
        // 
        // add delta time to simulation time and not processed time
        // not processed time is greater than the time required for a game turn
        //  yes)  execute a simulation turn
        //  no)   return
    }
    private void ResetState()
    {
        CurrentLockstepTurn = 0;
        CurrentGameTurn     = 0;
        SimulationTime      = Fix64.Zero;
        UnprocessedTime     = Fix64.Zero;

        lockstepIsOpen                   = false;
        lastGameTurnWhereLockstepWasOpen = 0;
    }
}