using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct Fix64Vec2
    {
        public static Fix64Vec2 zero { get { return Make(0L, 0L); } }
        public static Fix64Vec2 one { get { return Make(1L << FixedConstants64.SHIFT, 1L << FixedConstants64.SHIFT); } }

        public long RawX;
        public long RawY;

        public Fix64 x { get { return Fix64.FromRaw(RawX); } set { RawX = value.Raw; } }
        public Fix64 y { get { return Fix64.FromRaw(RawY); } set { RawY = value.Raw; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64Vec2 Make(long rawValueX, long rawValueY)
        {
            Fix64Vec2 r;
            r.RawX = rawValueX;
            r.RawY = rawValueY;
            return r;
        }

        public Fix64Vec2(Fix64 x, Fix64 y)
        {
            RawX = x.Raw;
            RawY = y.Raw;
        }

        //operators
        public static explicit operator Fix64Vec2(Vector2 value)
        {
            Fix64Vec2 r = Fix64Vec2.zero;

            r.x = (Fix64)value.x;
            r.y = (Fix64)value.y;

            return r;
        }

        public static explicit operator Fix64Vec2(Vector3 value)
        {
            Fix64Vec2 r = Fix64Vec2.zero;

            r.x = (Fix64)value.x;
            r.y = (Fix64)value.y;

            return r;
        }

        public static explicit operator Vector2(Fix64Vec2 value)
        {
            return new Vector2((float)value.x, (float)value.y);
        }

        public static explicit operator Fix64Vec2(Fix64Vec3 value)
        {
            return Fix64Vec2.Make(value.RawX, value.RawY);
        }

        public static Fix64Vec2 operator +(Fix64Vec2 a, Fix64Vec2 b)
        {
            long x = a.RawX + b.RawX;
            long y = a.RawY + b.RawY;

            Fix64Vec2 r = Make(x, y);
            return r;
        }

        public static Fix64Vec2 operator -(Fix64Vec2 a, Fix64Vec2 b)
        {
            long x = a.RawX - b.RawX;
            long y = a.RawY - b.RawY;

            Fix64Vec2 r = Make(x, y);
            return r;
        }

        public static Fix64Vec2 operator *(Fix64Vec2 a, Fix64Vec2 b)
        {
            long x = NativeFixedMath.Mul64(a.RawX, b.RawX);
            long y = NativeFixedMath.Mul64(a.RawY, b.RawY);

            Fix64Vec2 r = Make(x, y);
            return r;
        }

        public static Fix64Vec2 operator *(Fix64 a, Fix64Vec2 b)
        {
            return Make(NativeFixedMath.Mul64(a.Raw, b.RawX), NativeFixedMath.Mul64(a.Raw, b.RawY));
        }

        public static Fix64Vec2 operator *(Fix64Vec2 a, Fix64 b)
        {
            return Make(NativeFixedMath.Mul64(a.RawX, b.Raw), NativeFixedMath.Mul64(a.RawY, b.Raw));
        }

        public static Fix64Vec2 operator /(Fix64 a, Fix64Vec2 b)
        {
            return Make(NativeFixedMath.Div64(a.Raw, b.RawX), NativeFixedMath.Div64(a.Raw, b.RawY));
        }

        public static Fix64Vec2 operator /(Fix64Vec2 a, Fix64 b)
        {
            return Make(NativeFixedMath.Div64(a.RawX, b.Raw), NativeFixedMath.Div64(a.RawY, b.Raw));
        }


        public static bool operator ==(Fix64Vec2 a, Fix64Vec2 b) { return a.RawX == b.RawX && a.RawY == b.RawY; }
        public static bool operator !=(Fix64Vec2 a, Fix64Vec2 b) { return a.RawX != b.RawX || a.RawY != b.RawY; }

        public Fix64Vec2 normalized
        {
            get
            {
                Fix64Vec2 result = Fix64Vec2.zero;
                NativeParallel2D.Vec2Normalize64(this, ref result);
                return result;
            }
        }

        public Fix64 Length()
        {
            Fix64 result = Fix64.zero;
            NativeParallel2D.Vec2Length64(this, ref result);
            return result;
        }

        //Distance between two points
        public static Fix64 Distance(Fix64Vec2 a, Fix64Vec2 b)
        {
            return (a - b).Length();
        }

        //Dot product of two vectors
        public static Fix64 Dot(Fix64Vec2 a, Fix64Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }


        //Cross product of two vectors
        public static Fix64 Cross(Fix64Vec2 a, Fix64Vec2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Fix64 Angle(Fix64Vec2 a, Fix64Vec2 b)
        {
            Fix64 dot = Fix64Vec2.Dot(a.normalized, b.normalized);
            Fix64 clamped = Fix64Math.Clamp(dot, -Fix64.one, Fix64.one);
            Fix64 rad = Fix64.FromRaw(NativeFixedMath.Acos64(clamped.Raw));
            return rad * Fix64.RadToDegree;
        }

        public static Fix64Vec2 Intersection(Fix64Vec2 a1, Fix64Vec2 a2, Fix64Vec2 b1, Fix64Vec2 b2, out bool found)
        {
            Fix64 tmp = (b2.x - b1.x) * (a2.y - a1.y) - (b2.y - b1.y) * (a2.x - a1.x);

            if (tmp == Fix64.zero)
            {
                // No solution!
                found = false;
                return Fix64Vec2.zero;
            }

            Fix64 mu = ((a1.x - b1.x) * (a2.y - a1.y) - (a1.y - b1.y) * (a2.x - a1.x)) / tmp;

            found = true;

            return new Fix64Vec2(
                b1.x + (b2.x - b1.x) * mu,
                b1.y + (b2.y - b1.y) * mu
            );
        }


        public static Fix64Vec2 Intersection(Fix64Vec2 a1, Fix64Vec2 v, Fix64 range, Fix64Vec2 b1, Fix64Vec2 b2, out bool found)
        {
            Fix64Vec2 a2 = a1 + v.normalized * range;
            return Intersection(a1, a2, b1, b2, out found);
        }

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        //http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
        public static Fix64 DistanceToSegment(
    Fix64Vec2 pt, Fix64Vec2 p1, Fix64Vec2 p2, out Fix64Vec2 closest)
        {
            Fix64 dx = p2.x - p1.x;
            Fix64 dy = p2.y - p1.y;
            if ((dx == Fix64.zero) && (dy == Fix64.zero))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
                return Fix64Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            Fix64 t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < Fix64.zero)
            {
                closest = p1;
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > Fix64.one)
            {
                closest = p2;
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                closest = new Fix64Vec2(p1.x + t * dx, p1.y + t * dy);
                dx = pt.x - closest.x;
                dy = pt.y - closest.y;
            }

            return Fix64Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return $"Fix64Vec2({x}, {y})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fix64Vec2))
                return false;
            return ((Fix64Vec2)obj) == this;
        }

        public override int GetHashCode()
        {
            return RawX.GetHashCode() ^ RawY.GetHashCode() * 7919;
        }
    }
}
