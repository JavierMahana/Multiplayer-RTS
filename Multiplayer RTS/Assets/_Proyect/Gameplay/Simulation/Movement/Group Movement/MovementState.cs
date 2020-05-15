using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementState : IComponentData
{
    public bool PreviousStepDestiantionReached;

    public bool DestinationReached;
    public Fix64 DestinationIsReachedDistance;

    /// <summary>
    /// this is updated when the destinaion is reached. Don't use when "DestinationReached" = false.
    /// </summary>
    public Hex HexOcuppied;
}
