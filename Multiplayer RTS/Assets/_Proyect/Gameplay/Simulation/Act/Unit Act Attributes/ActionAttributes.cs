using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ActionAttributes : IComponentData
{
    /// <summary>
    /// esta distancia debe ser usada como distancia despues del radio.
    /// </summary>
    public Fix64 ActRange;
    
}
