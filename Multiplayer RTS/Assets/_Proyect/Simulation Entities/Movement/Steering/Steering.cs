using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Steering : IComponentData
{


    public int previousDirectionWeight;
    public int targetWeight;    
    public int flockWeight;
    //despues hacer una shared coomp


    public int cohesionWeight;
    public int alineationWeight;
    public int separationWeight;

    public Fix64 separationDistance;
    public Fix64 satisfactionDistance;
}
