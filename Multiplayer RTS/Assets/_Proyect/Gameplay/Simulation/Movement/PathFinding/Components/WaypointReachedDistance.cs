﻿using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct WaypointReachedDistance : IComponentData
{
    public Fix64 Value;
}
