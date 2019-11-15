using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public struct Orientation 
{


    public static Orientation pointy = new Orientation(
        Fix64.Sqrt((Fix64)3),  Fix64.Sqrt((Fix64)3) / (Fix64)2,
        (Fix64)0,  (Fix64)3 / (Fix64)2,

        Fix64.Sqrt((Fix64)3) / (Fix64)3, new Fix64(-1) / (Fix64)3,
        (Fix64)0, (Fix64)2 / (Fix64)3,

        (Fix64)0.5);
  //  const Orientation layout_flat
  //= Orientation(3.0 / 2.0, 0.0, sqrt(3.0) / 2.0, sqrt(3.0),
  //              2.0 / 3.0, 0.0, -1.0 / 3.0, sqrt(3.0) / 3.0,
  //              0.0);
    public Orientation(Fix64 f11, Fix64 f12, Fix64 f21, Fix64 f22, Fix64 d11, Fix64 d12, Fix64 d21, Fix64 d22, Fix64 angle)
    {
        // |11|12|
        // |21|22|
        this.f11 = f11;
        this.f12 = f12;
        this.f21 = f21;
        this.f22 = f22;
        this.d11 = d11;
        this.d12 = d12;
        this.d21 = d21;
        this.d22 = d22;
        this.angle = angle;
    }
    public readonly Fix64 f11;
    public readonly Fix64 f12;
    public readonly Fix64 f21;
    public readonly Fix64 f22;
    public readonly Fix64 d11;
    public readonly Fix64 d12;
    public readonly Fix64 d21;
    public readonly Fix64 d22;
    public readonly Fix64 angle;

}
