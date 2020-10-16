using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 24)]
    public struct Fix64Vec3
    {
        public static Fix64Vec3 zero    { get { return new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.zero); } }
        public static Fix64Vec3 one     { get { return new Fix64Vec3(Fix64.one, Fix64.one, Fix64.one); } }
        public static Fix64Vec3 down    { get { return new Fix64Vec3(Fix64.zero, Fix64.NegOne, Fix64.zero); } }
        public static Fix64Vec3 up      { get { return new Fix64Vec3(Fix64.zero, Fix64.one, Fix64.zero); } }
        public static Fix64Vec3 left    { get { return new Fix64Vec3(Fix64.NegOne, Fix64.zero, Fix64.zero); } }
        public static Fix64Vec3 right   { get { return new Fix64Vec3(Fix64.one, Fix64.zero, Fix64.zero); } }
        public static Fix64Vec3 forward { get { return new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.one); } }
        public static Fix64Vec3 back    { get { return new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.NegOne); } }
        public static Fix64Vec3 axisX   { get { return new Fix64Vec3(Fix64.one, Fix64.zero, Fix64.zero); } }
        public static Fix64Vec3 axisY   { get { return new Fix64Vec3(Fix64.zero, Fix64.one, Fix64.zero); } }
        public static Fix64Vec3 axisZ   { get { return new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.one); } }

        public long RawX;
        public long RawY;
        public long RawZ;

        public Fix64 x { get { return Fix64.FromRaw(RawX); } set { RawX = value.Raw; } }
        public Fix64 y { get { return Fix64.FromRaw(RawY); } set { RawY = value.Raw; } }
        public Fix64 z { get { return Fix64.FromRaw(RawZ); } set { RawZ = value.Raw; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64Vec3 Make(long rawValueX, long rawValueY, long rawValueZ)
        {
            Fix64Vec3 r;
            r.RawX = rawValueX;
            r.RawY = rawValueY;
            r.RawZ = rawValueZ;
            return r;
        }

        public Fix64Vec3(Fix64 x, Fix64 y, Fix64 z)
        {
            RawX = x.Raw;
            RawY = y.Raw;
            RawZ = z.Raw;
        }

        public static explicit operator Fix64Vec3(Vector3 value)
        {
            Fix64Vec3 r = Fix64Vec3.zero;
            
            r.x = (Fix64)value.x;
            r.y = (Fix64)value.y;
            r.z = (Fix64)value.z;

            return r;
        }

        public static explicit operator Vector3(Fix64Vec3 value)
        {
            return new Vector3((float)value.x, (float)value.y, (float)value.z);
        }

        public static explicit operator Fix64Vec3(Fix64Vec2 value)
        {
            return Fix64Vec3.Make(value.RawX, value.RawY, 0L);
        }

        public static Fix64Vec3 operator -(Fix64Vec3 a)
        {
            return Make(-a.RawX, -a.RawY, -a.RawZ);
        }

        public static Fix64Vec3 operator +(Fix64Vec3 a, Fix64Vec3 b)
        {
            long x = a.RawX + b.RawX;
            long y = a.RawY + b.RawY;
            long z = a.RawZ + b.RawZ;
            Fix64Vec3 r = Make(x, y, z);
            return r;
        }

        public static Fix64Vec3 operator -(Fix64Vec3 a, Fix64Vec3 b)
        {
            long x = a.RawX - b.RawX;
            long y = a.RawY - b.RawY;
            long z = a.RawZ - b.RawZ;
            Fix64Vec3 r = Make(x, y, z);
            return r;
        }

        public static Fix64Vec3 operator *(Fix64Vec3 a, Fix64Vec3 b)
        {
            long x = NativeFixedMath.Mul64(a.RawX, b.RawX);
            long y = NativeFixedMath.Mul64(a.RawY, b.RawY);
            long z = NativeFixedMath.Mul64(a.RawZ, b.RawZ);
            Fix64Vec3 r = Make(x, y, z);
            return r;
        }

        public static Fix64Vec3 operator /(Fix64Vec3 a, Fix64Vec3 b)
        {
            long x = NativeFixedMath.Div64(a.RawX, b.RawX);
            long y = NativeFixedMath.Div64(a.RawY, b.RawY);
            long z = NativeFixedMath.Div64(a.RawZ, b.RawZ);
            Fix64Vec3 r = Make(x, y, z);
            return r;
        }

        public static Fix64Vec3 operator *(Fix64 a, Fix64Vec3 b)
        {
            return Make(NativeFixedMath.Mul64(a.Raw, b.RawX), NativeFixedMath.Mul64(a.Raw, b.RawY), NativeFixedMath.Mul64(a.Raw, b.RawZ));
        }

        public static Fix64Vec3 operator *(Fix64Vec3 a, Fix64 b)
        {
            return Make(NativeFixedMath.Mul64(a.RawX, b.Raw), NativeFixedMath.Mul64(a.RawY, b.Raw), NativeFixedMath.Mul64(a.RawZ, b.Raw));
        }

        public static Fix64Vec3 operator /(Fix64 a, Fix64Vec3 b)
        {
            return Make(NativeFixedMath.Div64(a.Raw, b.RawX), NativeFixedMath.Div64(a.Raw, b.RawY), NativeFixedMath.Div64(a.Raw, b.RawZ));
        }

        public static Fix64Vec3 operator /(Fix64Vec3 a, Fix64 b)
        {
            return Make(NativeFixedMath.Div64(a.RawX, b.Raw), NativeFixedMath.Div64(a.RawY, b.Raw), NativeFixedMath.Div64(a.RawZ, b.Raw));
        }

        public static bool operator ==(Fix64Vec3 a, Fix64Vec3 b)
        {
            return a.RawX == b.RawX && a.RawY == b.RawY && a.RawZ == b.RawZ;
        }

        public static bool operator !=(Fix64Vec3 a, Fix64Vec3 b)
        {
            return a.RawX != b.RawX || a.RawY != b.RawY || a.RawZ != b.RawZ;
        }

        public static Fix64 LengthSqr(Fix64Vec3 a)
        {
            return Dot(a, a);
        }

        public Fix64Vec3 normalized
        {
            get
            {
                Fix64Vec3 result = Fix64Vec3.zero;
                NativeParallel3D.Vec3Normalize64(this, ref result);
                return result;
            }
        }

        public Fix64 Length()
        {
            Fix64 result = Fix64.zero;
            NativeParallel3D.Vec3Length64(this, ref result);
            return result;
        }

        //Projects vector a onto vector b
        public static Fix64Vec3 Project(Fix64Vec3 a, Fix64Vec3 b)
        {

            Fix64 dotB = Dot(b, b);
            if(dotB <= Fix64.zero)
            {
                return Fix64Vec3.zero;
            }
            return b * Dot(a, b) / dotB;
        }

        //Projects vector a ontp a plane defined by a normal orthogonal to the plane.
        public static Fix64Vec3 ProjectOnPlane(Fix64Vec3 a, Fix64Vec3 planeNormal)
        {
            return a - Project(a, planeNormal);
        }

        public override string ToString()
        {
            return $"Fix64Vec3({x}, {y}, {z})";
        }

        public static Fix64Vec3 ClampLength(Fix64Vec3 a, Fix64 max)
        {
            if(LengthSqr(a) > (max * max))
            {
                return a.normalized * max;
            }

            return a;
        }

        //Distance between two points
        public static Fix64 Distance(Fix64Vec3 a, Fix64Vec3 b)
        {
            return (a - b).Length();
        }

        //Dot product of two vectors
        public static Fix64 Dot(Fix64Vec3 a, Fix64Vec3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        //Cross product of two vectors
        public static Fix64Vec3 Cross(Fix64Vec3 a, Fix64Vec3 b)
        {
            Fix64 x = a.y * b.z - a.z * b.y;
            Fix64 y = a.z * b.x - a.x * b.z;
            Fix64 z = a.x * b.y - a.y * b.x;
            return new Fix64Vec3(x, y, z);
        }

        //Angle in degrees between two normalized vectors
        public static Fix64 Anlge(Fix64Vec3 a, Fix64Vec3 b)
        {
            Fix64 dot = Fix64Vec3.Dot(a.normalized, b.normalized);
            Fix64 clamped = Fix64Math.Clamp(dot, -Fix64.one, Fix64.one);
            Fix64 rad = Fix64.FromRaw(NativeFixedMath.Acos64(clamped.Raw));
            return rad * Fix64.RadToDegree;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fix64Vec3))
                return false;
            return ((Fix64Vec3)obj) == this;
        }

        public override int GetHashCode()
        {
            return RawX.GetHashCode() ^ RawY.GetHashCode() * 7919 ^ RawZ.GetHashCode() * 4513;
        }
    }
}
