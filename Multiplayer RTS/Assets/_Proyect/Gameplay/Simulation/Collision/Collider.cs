using FixMath.NET;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Flags]
public enum ColliderFlags : byte
{
    NONE = 0,
    UNITS = 1,
    GROUPS = 2
}
public enum ColliderLayer : short
{
    UNIT = 1,
    GROUP = 2
}
[Serializable]
public struct Collider : IComponentData
{
    public Fix64 Radius;
    public Fix64 CollisionResolutionFactor;
    /// <summary>
    /// Intensity of collision resolution in one simulation frame. Measured in Radious. Used in the collision with *unwalkable*
    /// </summary>
    public Fix64 CollisionPushIntensity;
    public ColliderLayer Layer;
}



