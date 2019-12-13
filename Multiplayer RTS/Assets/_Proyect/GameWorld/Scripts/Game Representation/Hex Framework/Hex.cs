using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FixMath;
using System;
using FixMath.NET;

public struct Hex : IEquatable<Hex>
{

    public readonly int q, r, s;
    //public Hex(int q, int r, int s)
    //{
    //    this.q = q;
    //    this.r = r;
    //    this.s = s;
    //    if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
    //}
    public Hex(int q, int r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;
        if ((int)math.round(this.q + this.r + this.s) != 0) throw new ArgumentException("q + r + s must be 0");
    }

    //   directions
    //  5   / \   0
    // 4   |   |   1
    //  3   \ /   2
    static readonly public Hex[] directions = new Hex[]
    {
        new Hex(1, 0), new Hex(1, -1), new Hex(0, -1), new Hex(-1, 0), new Hex(-1, 1), new Hex(0, 1)
    };
    public static readonly Hex Zero = new Hex(0,0);
    
    public static explicit operator FractionalHex(Hex h) 
    {
        return new FractionalHex((Fix64)h.q, (Fix64)h.r);
    }
    public static Hex operator -(Hex a)
    {
        return new Hex(-a.q, -a.r);
    }
    public static Hex operator +(Hex a, Hex b)
    {
        return new Hex(a.q + b.q, a.r + b.r);
    }
    public static Hex operator -(Hex a, Hex b)
    {
        return new Hex(a.q - b.q, a.r - b.r);
    }



    public static FractionalHex Lerp(FractionalHex a, FractionalHex b, Fix64 t)
    {      
        return new FractionalHex
            (
            Fix64.Lerp(a.q, b.q, t),
            Fix64.Lerp(a.r, b.r, t)
            );
    }
    public static FractionalHex Lerp(Hex a, Hex b, Fix64 t)    
    {
        return new FractionalHex
            (
            Fix64.Lerp((Fix64)a.q, (Fix64)b.q, t),
            Fix64.Lerp((Fix64)a.r, (Fix64)b.r, t)
            );
    }

    public static List<Hex> AllHexesInRange(int range, bool includeCenter = false)
    {
        int expected = 0;
        for (int i = 0; i <= range; i++)
        {
            expected += i * 6;
        }
        var hexesList = new List<Hex>(expected);

        for (int q = -range; q <= range; q++)
        {
            int startR = math.max(-range, -q - range);
            int endR = math.min(range, -q + range);
            for (int r = startR; r <= endR; r++)
            {
                if (!includeCenter && q == 0 && r == 0)
                {
                    continue;
                }
                hexesList.Add(new Hex(q,r));
            }
        }

        return hexesList;
    }
    public static List<Hex> AllHexesInRange(Hex startHex, int range, bool includeCenter = false)
    {
        int expected = 0;
        for (int i = 0; i <= range; i++)
        {
            expected += i * 6;
        }
        var hexesList = new List<Hex>(expected);

        for (int q = -range; q <= range; q++)
        {
            int startR = math.max(-range, -q - range);
            int endR = math.min(range, -q + range);
            for (int r = startR; r <= endR; r++)
            {
                if (!includeCenter && q == 0 && r == 0)
                {
                    continue;
                }
                hexesList.Add(new Hex(q + startHex.q, r + startHex.r));
            }
        }

        return hexesList;
    }

    public static List<Hex> HexesInBetween(Hex a, Hex b)
    {
        int hexDistance = a.Distance(b);

        FractionalHex a_nudge = new FractionalHex((Fix64)a.q + (Fix64)0.000001, (Fix64)a.r + (Fix64)0.000001);
        FractionalHex b_nudge = new FractionalHex((Fix64)b.q + (Fix64)0.000001, (Fix64)b.r + (Fix64)0.000001);

        List<Hex> results = new List<Hex>();


        Fix64 step = (Fix64)1 / (Fix64)Math.Max(hexDistance, 1);

        for (int i = 0; i <= hexDistance; i++)
        {
            results.Add( FractionalHex.Lerp( a_nudge, b_nudge, step * (Fix64)i).Round());
        }
        return results;
    }

    //este seria bueno de testear en situaciones extremas
    public static List<Hex> HexesInBetween(FractionalHex af, FractionalHex bf)
    {
        
        Hex a = af.Round();
        Hex b = bf.Round();

        return HexesInBetween(a, b);
        //int hexDistance = a.Distance(b);

        //List<Hex> results = new List<Hex> { };

        //Fix64 step = (Fix64)1 / (Fix64)Math.Max(hexDistance, 1);

        //for (int i = 0; i <= hexDistance; i++)
        //{
        //    results.Add(FractionalHex.Lerp(af, bf, step * (Fix64)i).Round());
        //}
        //return results;
    }


    public Hex Scale(int k)
    {
        return new Hex(q * k, r * k);

    }


    public Hex Neightbor(int side)
    {
        side %= 6;
        return this + directions[side];
    }

    public Fix64 Magnitude()
    {
        Fix64 sqrMag = Fix64.Pow(Fix64.Abs((Fix64)q), (Fix64)2) + Fix64.Pow(Fix64.Abs((Fix64)r), (Fix64)2) + Fix64.Pow(Fix64.Abs((Fix64)s), (Fix64)2);
        return Fix64.Sqrt(sqrMag);
    }
    public Fix64 SqrMagnitude()
    {
        return Fix64.Pow(Fix64.Abs((Fix64)q), (Fix64)2) + Fix64.Pow(Fix64.Abs((Fix64)r), (Fix64)2) + Fix64.Pow(Fix64.Abs((Fix64)s), (Fix64)2);         
    }
    public FractionalHex Normalized()
    {
        Fix64 mag = this.Magnitude();
        return new FractionalHex ((Fix64)q / mag,(Fix64)r / mag);
    }
    public FractionalHex NormalizedManhathan()
    {
        int lenght = this.Lenght();
        var normalized = lenght != 0 ? new FractionalHex((Fix64)(q / lenght), (Fix64)(r / lenght)) : FractionalHex.Zero;
        return normalized;
    }
    public int Lenght()
    {
        return ((math.abs(q) + math.abs(r) + math.abs(s)) / 2);
    }
    public int Distance(Hex b)
    {
        return (this - b).Lenght();
    }
    public bool Equals(Hex other)
    {
        if (other == this) return true;
        return false;
    }


    public override string ToString()
    {
        return $"Hex ({q}|{r}|{s})";
    }
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + q.GetHashCode();
            hash = hash * 23 + r.GetHashCode();
            hash = hash * 23 + s.GetHashCode();
            return hash;
        }
    }
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Hex)) return false;

        Hex p = (Hex)obj;
        return q == p.q && r == p.r && s == p.s;
    }
    public static bool operator ==(Hex a, Hex b)
    {
        return a.q == b.q && a.r == b.r && a.s == b.s;
    }
    public static bool operator !=(Hex a, Hex b)
    {
        return !(a.q == b.q && a.r == b.r && a.s == b.s);
    }

}
