using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
/// <summary>
/// componente que es usado para determinar si es que una unidad(UN GRUPO) eha llegado a su destino de movimiento o no.
/// </summary>
public struct MovementState : IComponentData
{
    /// <summary>
    /// actualizado en movmentfinisher system. usado para conocer cual es la frame que inicialmente llega a su destino.
    /// </summary>
    public bool PreviousStepDestiantionReached;

    public bool DestinationReached;
    public Fix64 DestinationIsReachedDistance;

    /// <summary>
    /// this is updated when the destinaion is reached. Don't use when "DestinationReached" = false.
    /// </summary>
    public Hex HexOcuppied;
}
