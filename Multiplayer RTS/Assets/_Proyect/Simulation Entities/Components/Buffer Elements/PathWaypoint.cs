using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public struct PathWaypoint : IBufferElementData
{
    public static implicit operator Hex(PathWaypoint e) { return e.Value; }
    public static implicit operator PathWaypoint(Hex e) { return new PathWaypoint { Value = e }; }


    public Hex Value;

}
