using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[SerializeField]
public struct MovementTarget : IComponentData
{
    public float3 TargetPostion;
}
