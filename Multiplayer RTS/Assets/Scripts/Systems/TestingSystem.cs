using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class TestingSystem : ComponentSystem
{    
    float3 savedValue;
    
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MovementTarget movement) =>
        {
            if (!movement.TargetPostion.Equals(savedValue))
            {
                savedValue = movement.TargetPostion;
                CommandListenertest.DisplayText(savedValue.ToString());
            }
        });
    }
}

     
