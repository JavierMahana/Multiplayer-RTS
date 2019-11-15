using FixMath.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FractionalHex 
{
    public readonly Fix64 q, r, s;
    public static readonly FractionalHex Zero = new FractionalHex(Fix64.Zero,Fix64.Zero);
    public FractionalHex(Fix64 q, Fix64 r, Fix64 s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if ((int)Fix64.Round(this.q + this.r + this.s) != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public FractionalHex(Fix64 q, Fix64 r)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;
        if ((int)Fix64.Round(this.q + this.r + this.s) != 0) throw new ArgumentException("q + r + s must be 0");
    }


    public static FractionalHex operator +(FractionalHex a, FractionalHex b)
    {
        return new FractionalHex(a.q + b.q, a.r + b.r);
    }
    public static FractionalHex operator -(FractionalHex a, FractionalHex b)
    {
        return new FractionalHex(a.q - b.q, a.r - b.r);
    }
    public static FractionalHex operator *(FractionalHex a, int b)
    {
        return new FractionalHex(a.q * (Fix64)b, a.r * (Fix64)b);
    }
    public static FractionalHex operator *(FractionalHex a, Fix64 b)
    {
        return new FractionalHex(a.q * b, a.r * b);
    }
    public static FractionalHex operator /(FractionalHex a, Fix64 b)
    {
        return new FractionalHex(a.q / b, a.r / b);
    }
    public static FractionalHex operator /(FractionalHex a, int b)
    {
        return new FractionalHex(a.q / (Fix64)b, a.r / (Fix64)b);
    }


    public Fix64 Lenght()
    {
        return ((Fix64.Abs(q) + Fix64.Abs(r) + Fix64.Abs(s)) / (Fix64)2);
    }
    public Fix64 Distance(FractionalHex b)
    {
        return (this - b).Lenght();
    }

    public Fix64 Magnitude()
    {
        Fix64 sqrMag = Fix64.Pow(Fix64.Abs(q), (Fix64)2) + Fix64.Pow(Fix64.Abs(r), (Fix64)2) + Fix64.Pow(Fix64.Abs(s), (Fix64)2);
        return Fix64.Sqrt(sqrMag);
    }
    public Fix64 SqrMagnitude()
    {
        return Fix64.Pow(Fix64.Abs(q), (Fix64)2) + Fix64.Pow(Fix64.Abs(r), (Fix64)2) + Fix64.Pow(Fix64.Abs(s), (Fix64)2);
    }
    public FractionalHex Normalized()
    {
        Fix64 mag = this.Magnitude();
        return new FractionalHex(q / mag, r / mag);
    }

    public static FractionalHex Lerp(FractionalHex a, FractionalHex b, Fix64 t)
    {
        return new FractionalHex(
            Fix64.Lerp(a.q, b.q, t),
            Fix64.Lerp(a.r, b.r, t)
            );
    }
    public Hex Round()
    {
        int qi = (int)(Fix64.Round(q));
        int ri = (int)(Fix64.Round(r));
        int si = (int)(Fix64.Round(s));
        Fix64 q_diff = Fix64.Abs((Fix64)qi - q);
        Fix64 r_diff = Fix64.Abs((Fix64)ri - r);
        Fix64 s_diff = Fix64.Abs((Fix64)si - s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            qi = -ri - si;
        }
        else if (r_diff > s_diff)
        {
            ri = -qi - si;
        }
        else
        {
            si = -qi - ri;
        }
        return new Hex(qi, ri);
    }
}
