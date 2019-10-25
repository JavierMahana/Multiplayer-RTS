using FixMath.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FractionalHex 
{
    public readonly Fix64 q, r, s;
    public FractionalHex(Fix64 q, Fix64 r, Fix64 s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if((int)Fix64.Round(this.q + this.r + this.s) != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public FractionalHex(Fix64 q, Fix64 r)
    {
        this.q = q;
        this.r = r;
        this.s = -q-r;
        if ((int)Fix64.Round(this.q + this.r + this.s) != 0) throw new ArgumentException("q + r + s must be 0");
    }

    public static FractionalHex Lerp(FractionalHex a, FractionalHex b, Fix64 t)
    {
        return new FractionalHex(
            Fix64.Lerp(a.q, b.q, t),
            Fix64.Lerp(a.r, b.r, t),
            Fix64.Lerp(a.s, b.s, t)
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
        else
            if (r_diff > s_diff)
        {
            ri = -qi - si;
        }
        else
        {
            si = -qi - ri;
        }
        return new Hex(qi, ri, si);
    }
}
