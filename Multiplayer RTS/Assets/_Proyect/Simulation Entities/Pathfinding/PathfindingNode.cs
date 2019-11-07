using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct PathfindingNode : IEquatable<PathfindingNode>, IEquatable<Hex>
{
    public PathfindingNode(Hex hex, int gCost, int hCost, Hex parent)
    {
        this.hex = hex;
        this.gCost = gCost;
        this.hCost = hCost;
        this.parent = parent;
    }
    public bool startNode => hex == parent;
    public int fCost => gCost + hCost;
    public readonly int gCost;
    public readonly int hCost;
    public readonly Hex parent;

    public readonly Hex hex;

    public static bool operator ==(PathfindingNode a, PathfindingNode b) 
    {
        return a.hex == b.hex;
    }
    public static bool operator !=(PathfindingNode a, PathfindingNode b)
    {
        return a.hex != b.hex;
    }
    public override int GetHashCode()
    {
        return hex.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        bool h = obj is Hex;
        bool n = obj is PathfindingNode;
        if ((h || n))
        {
            if (h) 
            {
                return (Hex)obj == this.hex;
            }
            if (n) 
            {
                return (PathfindingNode)obj == this;
            }
        }
        return false;
    }
    public bool Equals(PathfindingNode other)
    {
        return this == other;
    }

    public bool Equals(Hex other)
    {
        return this.hex == other;
    }
}
