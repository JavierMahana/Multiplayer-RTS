using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HasGatherer : IComponentData
{
    //TAG de los grupos de gatherers para saber que tienen gatherers como hijos.
}
