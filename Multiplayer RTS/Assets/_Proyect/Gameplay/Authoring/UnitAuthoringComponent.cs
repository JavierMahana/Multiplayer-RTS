using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using FixMath.NET;
using Sirenix.OdinInspector;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using Photon.Pun;

[DisallowMultipleComponent]
[RequireComponent(typeof(EntityFilter))]
//starts on reinforcement mode by default
public class UnitAuthoringComponent : BaseUnitAuthoringComponent
{
    //por ahora las unidades son genericas, por lo q no tienen mas atributos.

}
