using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FixMath.NET
{
    [Serializable]
    public struct FixVector2 
    {
        public Fix64 x, y;

        public static FixVector2 operator + (FixVector2 a, FixVector2 b)
        {
            return new FixVector2(a.x + b.x, a.y + b.y);
        }

        public FixVector2(Fix64 x, Fix64 y)
        {
            this.x = x;
            this.y = y;
        }
        public FixVector2(int x, int y)
        {
            this.x = (Fix64)x;
            this.y = (Fix64)y;
        }
    }
}
