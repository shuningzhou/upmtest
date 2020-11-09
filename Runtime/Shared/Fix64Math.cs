using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Parallel
{
    public static class Fix64Math
    {
        //Fix64
        public static Fix64 Sqrt(Fix64 value)
        {
            long sqrtValue = NativeFixedMath.Sqrt64(value.Raw);
            return Fix64.FromRaw(sqrtValue);
        }

        public static Fix64 Max(Fix64 a, Fix64 b)
        {
            return a > b ? a : b;
        }

        public static Fix64 Max(Fix64 a, Fix64 b, Fix64 c)
        {
            return Max(a, Max(b, c));
        }

        public static Fix64 Min(Fix64 a, Fix64 b)
        {
            return a < b ? a : b;
        }

        public static Fix64 Clamp(Fix64 a, Fix64 low, Fix64 high)
        {
            return Max(low, Min(a, high));
        }

        public static Fix64 Lerp(Fix64 a, Fix64 b, Fix64 t)
        {
            t = Clamp(t, Fix64.zero, Fix64.one);
            return a + (b - a) * t;
        }

        public static Fix64 LerpUnClamped(Fix64 a, Fix64 b, Fix64 t)
        {
            return a + (b - a) * t;
        }

        //Fix64Vec2

        public static Fix64Vec2 FindNearestPointOnLine(Fix64Vec2 p, Fix64Vec2 a, Fix64Vec2 b)
        {
            Fix64Vec2 ba = b - a;

            Fix64Vec2 pa = p - a;
            Fix64 d = pa.Length();

            Fix64 angle = Fix64Vec2.Angle(ba, pa);
            if (angle > Fix64.FromDivision(90, 1))
            {
                angle = Fix64.FromDivision(90, 1);
            }
            d = d * Fix64.Cos(angle * Fix64.DegreeToRad);

            return a + ba.normalized * d;
        }

        public static bool InSpan(Fix64Vec2 v, Fix64Vec2 va, Fix64Vec2 vb)
        {
            Fix64 AXB = Fix64Vec2.Cross(va, vb);
            Fix64 BXA = Fix64Vec2.Cross(vb, va);

            Fix64 AXV = Fix64Vec2.Cross(va, v);
            Fix64 BXV = Fix64Vec2.Cross(vb, v);

            if (AXV * AXB >= Fix64.zero && BXV * BXA >= Fix64.zero)
            {
                return true;
            }

            return false;
        }

        //Fix64Vec3
        public static Fix64Vec3 Mul(Fix64Vec3 pos, Fix64Quat rot, Fix64Vec3 point)
        {
            Fix64Vec3 output = Fix64Vec3.zero;
            NativeParallel3D.Mul(pos, rot, point, ref output);
            return output;
        }

        public static Fix64Vec3 MulT(Fix64Vec3 pos, Fix64Quat rot, Fix64Vec3 point)
        {
            Fix64Vec3 output = Fix64Vec3.zero;
            NativeParallel3D.MulT(pos, rot, point, ref output);
            return output;
        }
    }
}
