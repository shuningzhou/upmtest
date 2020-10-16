using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    public struct Fix64Vec4
    {
        public static Fix64Vec4 zero { get { return new Fix64Vec4(Fix64.zero, Fix64.zero, Fix64.zero, Fix64.zero); } }
        public static Fix64Vec4 one { get { return new Fix64Vec4(Fix64.one, Fix64.one, Fix64.one, Fix64.one); } }

        public long RawX;
        public long RawY;
        public long RawZ;
        public long RawW;

        public Fix64 x { get { return Fix64.FromRaw(RawX); } set { RawX = value.Raw; } }
        public Fix64 y { get { return Fix64.FromRaw(RawY); } set { RawY = value.Raw; } }
        public Fix64 z { get { return Fix64.FromRaw(RawZ); } set { RawZ = value.Raw; } }
        public Fix64 w { get { return Fix64.FromRaw(RawW); } set { RawW = value.Raw; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64Vec4 Make(long rawValueX, long rawValueY, long rawValueZ, long rawValueW)
        {
            Fix64Vec4 r;
            r.RawX = rawValueX;
            r.RawY = rawValueY;
            r.RawZ = rawValueZ;
            r.RawW = rawValueW;
            return r;
        }

        public Fix64Vec4(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            RawX = x.Raw;
            RawY = y.Raw;
            RawZ = z.Raw;
            RawW = w.Raw;
        }

        public static Fix64Vec4 operator -(Fix64Vec4 a)
        {
            return Make(-a.RawX, -a.RawY, -a.RawZ, -a.RawW);
        }

        public static Fix64Vec4 operator +(Fix64Vec4 a, Fix64Vec4 b)
        {
            return Make(a.RawX + b.RawX, a.RawY + b.RawY, a.RawZ + b.RawZ, a.RawW + b.RawW);
        }

        public static Fix64Vec4 operator -(Fix64Vec4 a, Fix64Vec4 b)
        {
            return Make(a.RawX - b.RawX, a.RawY - b.RawY, a.RawZ - b.RawZ, a.RawW - b.RawW);
        }

        public static Fix64Vec4 operator *(Fix64Vec4 a, Fix64Vec4 b)
        {
            return Make(NativeFixedMath.Mul64(a.RawX, b.RawX), NativeFixedMath.Mul64(a.RawY, b.RawY), NativeFixedMath.Mul64(a.RawZ, b.RawZ), NativeFixedMath.Mul64(a.RawW, b.RawW));
        }

        public static Fix64Vec4 operator /(Fix64Vec4 a, Fix64Vec4 b)
        {
            return Make(NativeFixedMath.Div64(a.RawX, b.RawX), NativeFixedMath.Div64(a.RawY, b.RawY), NativeFixedMath.Div64(a.RawZ, b.RawZ), NativeFixedMath.Div64(a.RawW, b.RawW));
        }

        public static Fix64Vec4 operator +(Fix64 a, Fix64Vec4 b)
        {
            return Make(a.Raw + b.RawX, a.Raw + b.RawY, a.Raw + b.RawZ, a.Raw + b.RawW);
        }

        public static Fix64Vec4 operator +(Fix64Vec4 a, Fix64 b)
        {
            return Make(a.RawX + b.Raw, a.RawY + b.Raw, a.RawZ + b.Raw, a.RawW + b.Raw);
        }

        public static Fix64Vec4 operator -(Fix64 a, Fix64Vec4 b)
        {
            return Make(a.Raw - b.RawX, a.Raw - b.RawY, a.Raw - b.RawZ, a.Raw - b.RawW);
        }

        public static Fix64Vec4 operator -(Fix64Vec4 a, Fix64 b)
        {
            return Make(a.RawX - b.Raw, a.RawY - b.Raw, a.RawZ - b.Raw, a.RawW - b.Raw);
        }

        public static Fix64Vec4 operator *(Fix64 a, Fix64Vec4 b)
        {
            return Make(NativeFixedMath.Mul64(a.Raw, b.RawX), NativeFixedMath.Mul64(a.Raw, b.RawY), NativeFixedMath.Mul64(a.Raw, b.RawZ), NativeFixedMath.Mul64(a.Raw, b.RawW));
        }

        public static Fix64Vec4 operator *(Fix64Vec4 a, Fix64 b)
        {
            return Make(NativeFixedMath.Mul64(a.RawX, b.Raw), NativeFixedMath.Mul64(a.RawY, b.Raw), NativeFixedMath.Mul64(a.RawZ, b.Raw), NativeFixedMath.Mul64(a.RawW, b.Raw));
        }

        public static Fix64Vec4 operator /(Fix64 a, Fix64Vec4 b)
        {
            return Make(NativeFixedMath.Div64(a.Raw, b.RawX), NativeFixedMath.Div64(a.Raw, b.RawY), NativeFixedMath.Div64(a.Raw, b.RawZ), NativeFixedMath.Div64(a.Raw, b.RawW));
        }

        public static Fix64Vec4 operator /(Fix64Vec4 a, Fix64 b)
        {
            return Make(NativeFixedMath.Div64(a.RawX, b.Raw), NativeFixedMath.Div64(a.RawY, b.Raw), NativeFixedMath.Div64(a.RawZ, b.Raw), NativeFixedMath.Div64(a.RawW, b.Raw));
        }

        public override string ToString()
        {
            return $"Fix64Vec4({x}, {y}, {z}, {w})";
        }
    }
}
