using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using System.Collections.Generic;

[UpdateBefore(typeof(LockstepLockSystem))]
public class LockstepCheckSystem : ComponentSystem
{
    private static bool logg = false;
    
    private static HashSet<int> m_ConfirmationChecks = new HashSet<int>();
    private static HashSet<int> m_CommandChecks = new HashSet<int>();

    public static bool AllCheksOfTurnAreRecieved(int turnToCheck)
    {
        if (turnToCheck < LockstepLockSystem.NUMBER_OF_TURNS_IN_THE_FUTURE_THE_COMMANDS_EXECUTE)
            return true;

        bool commandCheckReceived = m_CommandChecks.Contains(turnToCheck);
        bool confirmCheckReceived = m_ConfirmationChecks.Contains(turnToCheck);

        bool returnValue;
        if (OfflineMode.OffLineMode)
        {
            returnValue = true;
        }
        else
        {
            if (logg) Debug.Log($"the result of the online prossesing of the turn:{turnToCheck} is : {commandCheckReceived && confirmCheckReceived}");
            returnValue = commandCheckReceived && confirmCheckReceived;
        }

        if (returnValue) 
        {
            m_CommandChecks.Remove(turnToCheck);
            m_ConfirmationChecks.Remove(turnToCheck);
        }
        return returnValue;
    }





    protected override void OnUpdate()
    {
        if (logg) Debug.Log($"Lockstep Check system running on: {TimeSystem.TotalSimulationTime}");
        Entities.ForEach((Entity entity, ref LockstepCkeck check) =>
        {
            switch (check.Type)
            {
                case LockstepCheckType.COMMAND:
                    m_CommandChecks.Add(check.Turn);
                    break;
                case LockstepCheckType.CONFIRMATION:
                    m_ConfirmationChecks.Add(check.Turn);
                    break;
                default:
                    Debug.LogError("Check with invalid type");
                    break;
            }

            //check entity destroyed
            EntityManager.DestroyEntity(entity);
        });
    }
}