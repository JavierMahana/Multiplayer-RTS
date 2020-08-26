using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ActionTarget : IComponentData
{
    public Entity TargetEntity;
    public FractionalHex TargetPosition;
    public Fix64 TargetRadius;
    public bool IsUnit;



    public bool OccupiesFullHex;
    /// <summary>
    /// Solo usado cuando se esta recolectando recursos, por lo que el target es un recurso o un dropoffPoint.
    /// </summary>
    public bool GatherTarget;
    public bool IsResource;
    //agregar un booleano podria arreglar que diferencie si es un drop point o un recurso
}
