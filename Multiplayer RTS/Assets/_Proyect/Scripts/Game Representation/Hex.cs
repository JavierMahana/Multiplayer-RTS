using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FixMath;
using System;
using FixMath.NET;

public struct Hex 
{
    public readonly int q, r, s;
    public Hex(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
    }
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
        new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
    };


    public static explicit operator FractionalHex(Hex h) 
    {
        return new FractionalHex((Fix64)h.q, (Fix64)h.r);
    }
    public static Hex operator +(Hex a, Hex b)
    {
        return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
    }
    public static Hex operator -(Hex a, Hex b)
    {
        return new Hex(a.q - b.q, b.r - b.r, a.s - b.s);
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
    public static List<Hex> HexesInBetween(Hex a, Hex b)
    {
        int N = a.Distance(b);

        FractionalHex a_nudge = new FractionalHex((Fix64)a.q + (Fix64)0.000001, (Fix64)a.r + (Fix64)0.000001);
        FractionalHex b_nudge = new FractionalHex((Fix64)b.q + (Fix64)0.000001, (Fix64)b.r + (Fix64)0.000001);

        List<Hex> results = new List<Hex>{};


        Fix64 step = (Fix64)1 / (Fix64)Math.Max(N, 1);

        for (int i = 0; i <= N; i++)
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
        int N = a.Distance(b);

        List<Hex> results = new List<Hex> { };


        Fix64 step = (Fix64)1 / (Fix64)Math.Max(N, 1);

        for (int i = 0; i <= N; i++)
        {
            results.Add(FractionalHex.Lerp(af, bf, step * (Fix64)i).Round());
        }
        return results;
    }


    public Hex Scale(int k)
    {
        return new Hex(q * k, r * k, s * k);

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
    public int Lenght()
    {
        return ((math.abs(q) + math.abs(r) + math.abs(s)) / 2);
    }
    public int Distance(Hex b)
    {
        return (this - b).Lenght();
    }

}
