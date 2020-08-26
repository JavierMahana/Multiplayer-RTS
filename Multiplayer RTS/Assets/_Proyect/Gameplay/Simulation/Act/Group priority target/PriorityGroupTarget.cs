using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



[Serializable]
//Su utilidad es que genera un path de movimiento para que el grupo se mueva hacia este.
//y al mismo tiempo las unidades de manera independiente eligen objetivos para atacar.
public struct PriorityGroupTarget : IComponentData
{
    //public Entity TargetEntity;
    //public FractionalHex TargetPosition;
    public Hex TargetHex;
    /// <summary>
    /// en desuso.
    /// </summary>
    //public bool IsUnit;
}
