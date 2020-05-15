using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



[Serializable]
//hay dos opciones_- que el target sea unidad o estructura.
//lo que es realmente importante de esto es las concecuencias que genera:
//este componente altera la selección de posibles targets:
//1- la selección de posibles targets son todas las entidades pertenecientes al mismo grupo al que pertenecen la entidad que se tiene ahora.
//1.1- la selcción anterior debe considerar que tipo de entidad es la que se selecciona.


    //debe existir un sistema el cual, si estas en modo agresivo, revisa el ambiente y si hay cualquier blanco en rango agrega este componente,
    //si no encuentra ningún blanco o no esta en modo agresivo, se debe eliminar este componente.


    //PODRIAN HABER DOS COMPONENTES-> UNO SI EL TARGET ES UNA ESTRUCTURA Y OTRO SI ES UNA UNIDAD
public struct PriorityGroupTarget : IComponentData
{
    public Entity TargetEntity;
    public FractionalHex TargetPosition;
    public Hex TargetHex;
}
