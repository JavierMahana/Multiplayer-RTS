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
    public static FractionalHex operator -(FractionalHex a)
    {
        return new FractionalHex(-a.q , -a.r);
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
        var normalized = mag != Fix64.Zero ? new FractionalHex(q / mag, r / mag) : FractionalHex.Zero ;
        return normalized;
    }
    public FractionalHex NormalizedManhathan()
    {
        Fix64 lenght = this.Lenght();
        var normalized = lenght != Fix64.Zero ? new FractionalHex(q / lenght, r / lenght) : FractionalHex.Zero;
        return normalized;
    }
    public static Fix64 Angle(FractionalHex origin, FractionalHex direction, FractionalHex x, bool inRadians = true)
    {
        var diference = x - origin;
        direction = direction.Normalized();

        var adjacent = DotProduct(direction, diference);
        var hipotenuse = diference.Magnitude();
        var cos = hipotenuse != Fix64.Zero? adjacent / hipotenuse: Fix64.One;

        cos = cos.Clamp(Fix64.One, -Fix64.One);

        if (inRadians)
        {
            return Fix64.Acos(cos);
        }
        else 
        {
            return Fix64.Acos(Fix64.Degrees(cos));
        }        
    }
    public static FractionalHex Lerp(FractionalHex a, FractionalHex b, Fix64 t)
    {
        return new FractionalHex(
            Fix64.Lerp(a.q, b.q, t),
            Fix64.Lerp(a.r, b.r, t)
            );
    }
    public static Fix64 DotProduct(FractionalHex a, FractionalHex b)
    {
        return (a.q * b.q) + (a.r * b.r) + (a.s * b.s);
    }
    /// <summary>
    /// it returns the closest point along the direction to the point. the return point is at 90° from the start and the dest point
    /// </summary>
    /// <param name="lineOrigin"></param>
    /// <param name="lineDirection"></param>
    /// <param name="targetPoint"></param>
    /// <returns>it returns the closest point along the direction to the point. the return point is at 90° from the start and the dest point</returns>
    public static Fix64 ClosestPointInLine(FractionalHex lineOrigin, FractionalHex lineDirection, FractionalHex targetPoint)
    {
        var directionToPointNormalized = (targetPoint - lineOrigin).Normalized();
        var lineDirectionNormalized = lineDirection.Normalized();
        var cos = DotProduct(lineDirectionNormalized, directionToPointNormalized);//->  A*B / |A|*|B|

        var proyectionLenght = cos * (targetPoint - lineOrigin).Lenght();
        return proyectionLenght;
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
        if (obj == null || !(obj is Hex) && !(obj is FractionalHex)) return false;

        FractionalHex p = (FractionalHex)obj;
        return q == p.q && r == p.r && s == p.s;
    }
    public static bool operator ==(FractionalHex a, FractionalHex b)
    {
        return a.q == b.q && a.r == b.r && a.s == b.s;
    }
    public static bool operator !=(FractionalHex a, FractionalHex b)
    {
        return !(a.q == b.q && a.r == b.r && a.s == b.s);
    }
}
