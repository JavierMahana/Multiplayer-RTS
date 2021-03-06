﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FixMath.NET
{
    [Serializable]
    public struct FixVector2 
    {
        public Fix64 x, y;

        public static explicit operator FixVector2(Vector3 v) 
        {
            return new FixVector2((Fix64) v.x, (Fix64)v.y);
        }
        public static FixVector2 operator *(FixVector2 a, float b)
        {
            return new FixVector2(a.x * (Fix64)b, a.y * (Fix64)b);
        }
        public static FixVector2 operator *(FixVector2 a, double b)
        {
            return new FixVector2(a.x * (Fix64)b, a.y * (Fix64)b);
        }
        public static FixVector2 operator *(FixVector2 a, Fix64 b)
        {
            return new FixVector2(a.x * b, a.y * b);
        }
        public static FixVector2 operator *(FixVector2 a, int b)
        {
            return new FixVector2(a.x * (Fix64)b, a.y * (Fix64)b);
        }
        public static FixVector2 operator *(FixVector2 a, FixVector2 b)
        {
            return new FixVector2(a.x * b.x, a.y * b.y);
        }
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
