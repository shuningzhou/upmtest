using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct Fix64Quat
    {
        public static Fix64Quat identity { get { return Make(0L, 0L, 0L, 1L << FixedConstants64.SHIFT); } }

        public long RawX;
        public long RawY;
        public long RawZ;
        public long RawW;

        public Fix64 x { get { return Fix64.FromRaw(RawX); } set { RawX = value.Raw; } }
        public Fix64 y { get { return Fix64.FromRaw(RawY); } set { RawY = value.Raw; } }
        public Fix64 z { get { return Fix64.FromRaw(RawZ); } set { RawZ = value.Raw; } }
        public Fix64 w { get { return Fix64.FromRaw(RawW); } set { RawW = value.Raw; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fix64Quat Make(long rawValueX, long rawValueY, long rawValueZ, long rawValueW)
        {
            Fix64Quat r;
            r.RawX = rawValueX;
            r.RawY = rawValueY;
            r.RawZ = rawValueZ;
            r.RawW = rawValueW;
            return r;
        }

        public Fix64Quat(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            RawX = x.Raw;
            RawY = y.Raw;
            RawZ = z.Raw;
            RawW = w.Raw;
        }

        public static explicit operator Fix64Quat(Quaternion value)
        {
            Fix64Quat r = Fix64Quat.identity;

            r.x = (Fix64)value.x;
            r.y = (Fix64)value.y;
            r.z = (Fix64)value.z;
            r.w = (Fix64)value.w;

            return r;
        }

        public static explicit operator Quaternion(Fix64Quat value)
        {
            return new Quaternion((float)value.x, (float)value.y, (float)value.z, (float)value.w);
        }
        //zxy for unity
        //https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        /*
         * 
        threeaxisrot( -2*(q.x* q.y - q.w* q.z),
                      q.w* q.w - q.x* q.x + q.y* q.y - q.z* q.z,
                      2*(q.y* q.z + q.w* q.x),
                     -2*(q.x* q.z - q.w* q.y),
                      q.w* q.w - q.x* q.x - q.y* q.y + q.z* q.z,
                      res);

        void threeaxisrot(double r11, double r12, double r21, double r31, double r32, double res[]){
            res[0] = atan2( r31, r32 );
            res[1] = asin ( r21 );
            res[2] = atan2( r11, r12 );
        }

        r11 = -2*(q.x* q.y - q.w* q.z)
        r12 = q.w* q.w - q.x* q.x + q.y* q.y - q.z* q.z
        r21 = 2*(q.y* q.z + q.w* q.x)
        r31 = -2*(q.x* q.z - q.w* q.y)
        r32 =  q.w* q.w - q.x* q.x - q.y* q.y + q.z* q.z

        for zxy
        y = res[0]
        x = res[1]
        z = res[2]
        */
        public Fix64 GetXAngle()
        {
            //Fix64 r21 = (Fix64)2 * (y * z + w * x);
            //return Fix64.Asin(r21);

            Fix64 sinr_cosp = (Fix64)2 * (w * x + y * z);
            Fix64 cosr_cosp = (Fix64)1 - (Fix64)2 * (x * x + y * y);
            return Fix64.Atan2(sinr_cosp, cosr_cosp);
        }

        public Fix64 GetYAngle()
        {
            //Fix64 r31 = -(Fix64)2 * (x * z - w * y);
            //Fix64 r32 = w * w - x * x - y * y + z * z;
            //return Fix64.Atan2(r31, r32);
            Fix64 sinp = (Fix64)2 * (w * y - z * x);

            //if (Fix64.Abs(sinp) >= Fix64.one)
            //{
            //    Fix64 sign = Fix64.Sign(sinp);
            //    return sign * Fix64.halfPi;
            //}

            return Fix64.Asin(sinp);
        }

        public Fix64 GetZAngle()
        {
            //Fix64 r11 = -(Fix64)2 * (x * y - w * z);
            //Fix64 r12 = w * w - x * x + y * y - z * z;
            //return Fix64.Atan2(r11, r12);
            Fix64 siny_cosp = (Fix64)2 * (w * z + x * y);
            Fix64 cosy_cosp = (Fix64)1 - (Fix64)2 * (y * y + z * z);
            return Fix64.Atan2(siny_cosp, cosy_cosp);
        }

        public Fix64Vec3 EulerAngles()
        {
            Fix64 xDegree = Fix64.RadToDeg(GetXAngle());
            Fix64 yDegree = Fix64.RadToDeg(GetYAngle());
            Fix64 zDegree = Fix64.RadToDeg(GetZAngle());

            return new Fix64Vec3(xDegree, yDegree, zDegree);
        }

        //https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        public Fix64Vec3 EulerAngles1()
        {
            Fix64 sqw = w * w;
            Fix64 sqx = x * x;
            Fix64 sqy = y * y;
            Fix64 sqz = z * z;
            Fix64 unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            Fix64 test = x * y + z * w;

            Fix64 _x = Fix64.zero;
            Fix64 _y = Fix64.zero;
            Fix64 _z = Fix64.zero;

            Fix64 cutoff = Fix64.FromDivision(499, 1000) * unit;

            _z = Fix64.Atan2((Fix64)2 * y * w - (Fix64)2 * x * z, sqx - sqy - sqz + sqw);
            _y = Fix64.Asin((Fix64)2 * test / unit);
            _x = Fix64.Atan2((Fix64)2 * x * w - (Fix64)2 * y * z, -sqx + sqy - sqz + sqw);

            //if (test >= cutoff)
            //{ // singularity at north pole
            //    _z = (Fix64)2 * Fix64.Atan2(x, w);
            //    _y = Fix64.halfPi;
            //    _x = Fix64.zero;
            //}
            //else if (test < -cutoff)
            //{ // singularity at south pole
            //    _z = -(Fix64)2 * Fix64.Atan2(x, w);
            //    _y = -Fix64.halfPi;
            //    _x = Fix64.zero;
            //}
            //else
            //{
            //    _z = Fix64.Atan2((Fix64)2 * y * w - (Fix64)2 * x * z, sqx - sqy - sqz + sqw);
            //    _y = Fix64.Asin((Fix64)2 * test / unit);
            //    _x = Fix64.Atan2((Fix64)2 * x * w - (Fix64)2 * y * z, -sqx + sqy - sqz + sqw);
            //}
            Fix64 xDegree = Fix64.RadToDeg(_x);
            Fix64 yDegree = Fix64.RadToDeg(_y);
            Fix64 zDegree = Fix64.RadToDeg(_z);

            return new Fix64Vec3(xDegree, zDegree, yDegree);
        }

        public static Fix64Quat FromEulerAngles(Fix64Vec3 value)
        {
            Fix64 yaw_y = Fix64.DegToRad(value.y);
            Fix64 pitch_x = Fix64.DegToRad(value.x);
            Fix64 roll_z = Fix64.DegToRad(value.z);

            return Fix64Quat.FromYawPitchRoll(yaw_y, pitch_x, roll_z);
        }

        //https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        public static Fix64Quat FromYawPitchRoll(Fix64 yaw_y, Fix64 pitch_x, Fix64 roll_z)
        {
            //Fix64 half_yaw = Fix64.Div2(yaw_y);
            //Fix64 cy = Fix64.Cos(half_yaw);
            //Fix64 sy = Fix64.Sin(half_yaw);

            //Fix64 half_pitch = Fix64.Div2(pitch_x);
            //Fix64 cp = Fix64.Cos(half_pitch);
            //Fix64 sp = Fix64.Sin(half_pitch);

            //Fix64 half_roll = Fix64.Div2(roll_z);
            //Fix64 cr = Fix64.Cos(half_roll);
            //Fix64 sr = Fix64.Sin(half_roll);

            //Fix64 w = cr * cp * cy + sr * sp * sy;
            //Fix64 x = sr * cp * cy - cr * sp * sy;
            //Fix64 y = cr * sp * cy + sr * cp * sy;
            //Fix64 z = cr * cp * sy - sr * sp * cy;

            //return new Fix64Quat(x, y, z, w);

            Fix64 half_roll = Fix64.Div2(roll_z);
            Fix64 sr = Fix64.Sin(half_roll);
            Fix64 cr = Fix64.Cos(half_roll);

            Fix64 half_pitch = Fix64.Div2(pitch_x);
            Fix64 sp = Fix64.Sin(half_pitch);
            Fix64 cp = Fix64.Cos(half_pitch);

            Fix64 half_yaw = Fix64.Div2(yaw_y);
            Fix64 sy = Fix64.Sin(half_yaw);
            Fix64 cy = Fix64.Cos(half_yaw);

            return new Fix64Quat(
                cy * sp * cr + sy * cp * sr,
                sy * cp * cr - cy * sp * sr,
                cy * cp * sr - sy * sp * cr,
                cy * cp * cr + sy * sp * sr);
        }

        public static Fix64Quat operator *(Fix64Quat a, Fix64Quat b)
        {
            Fix64 q1x = a.x;
            Fix64 q1y = a.y;
            Fix64 q1z = a.z;
            Fix64 q1w = a.w;

            Fix64 q2x = b.x;
            Fix64 q2y = b.y;
            Fix64 q2z = b.z;
            Fix64 q2w = b.w;

            // cross(av, bv)
            Fix64 cx = q1y * q2z - q1z * q2y;
            Fix64 cy = q1z * q2x - q1x * q2z;
            Fix64 cz = q1x * q2y - q1y * q2x;

            Fix64 dot = q1x * q2x + q1y * q2y + q1z * q2z;

            return new Fix64Quat(
                q1x * q2w + q2x * q1w + cx,
                q1y * q2w + q2y * q1w + cy,
                q1z * q2w + q2z * q1w + cz,
                q1w * q2w - dot);
        }

        public static Fix64Vec3 operator *(Fix64Quat rotation, Fix64Vec3 point)
        {
            Fix64 num1 = rotation.x * Fix64.two;
            Fix64 num2 = rotation.y * Fix64.two;
            Fix64 num3 = rotation.z * Fix64.two;
            Fix64 num4 = rotation.x * num1;
            Fix64 num5 = rotation.y * num2;
            Fix64 num6 = rotation.z * num3;
            Fix64 num7 = rotation.x * num2;
            Fix64 num8 = rotation.x * num3;
            Fix64 num9 = rotation.y * num3;
            Fix64 num10 = rotation.w * num1;
            Fix64 num11 = rotation.w * num2;
            Fix64 num12 = rotation.w * num3;

            Fix64 x =(Fix64.one - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            Fix64 y = (num7 + num12) * point.x + (Fix64.one - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            Fix64 z = (num8 - num11) * point.x + (num9 + num10) * point.y + (Fix64.one - (num4 + num5)) * point.z;

            Fix64Vec3 vector3 = new Fix64Vec3(x, y, z);
            return vector3;
        }

        public static bool operator ==(Fix64Quat a, Fix64Quat b)
        {
            return a.RawX == b.RawX && a.RawY == b.RawY && a.RawZ == b.RawZ && a.RawW == b.RawW;
        }

        public static bool operator !=(Fix64Quat a, Fix64Quat b)
        {
            return a.RawX != b.RawX || a.RawY != b.RawY || a.RawZ != b.RawZ || a.RawW != b.RawW;
        }

        public static Fix64Quat Normalize(Fix64Quat a)
        {
            long inv_norm = NativeFixedMath.Rcp64(LengthFastest(a).Raw);
            Fix64 x = Fix64.FromRaw(NativeFixedMath.Mul64(a.RawX, inv_norm));
            Fix64 y = Fix64.FromRaw(NativeFixedMath.Mul64(a.RawY, inv_norm));
            Fix64 z = Fix64.FromRaw(NativeFixedMath.Mul64(a.RawZ, inv_norm));
            Fix64 w = Fix64.FromRaw(NativeFixedMath.Mul64(a.RawW, inv_norm));

            return new Fix64Quat(x, y, z, w);
        }

        public static Fix64 LengthSqr(Fix64Quat a)
        {
            return Fix64.FromRaw(NativeFixedMath.Mul64(a.RawX, a.RawX) 
                + NativeFixedMath.Mul64(a.RawY, a.RawY) 
                + NativeFixedMath.Mul64(a.RawZ, a.RawZ) 
                + NativeFixedMath.Mul64(a.RawW, a.RawW));
        }

        public static Fix64 LengthFastest(Fix64Quat a)
        {
            return Fix64.FromRaw(NativeFixedMath.Sqrt64(LengthSqr(a).Raw));
        }

        public static Fix64Quat Inverse(Fix64Quat a)
        {
            long inv_norm = NativeFixedMath.Rcp64(LengthSqr(a).Raw);
            return Make(
                -NativeFixedMath.Mul64(a.RawX, inv_norm),
                -NativeFixedMath.Mul64(a.RawY, inv_norm),
                -NativeFixedMath.Mul64(a.RawZ, inv_norm),
                NativeFixedMath.Mul64(a.RawW, inv_norm));
        }

        public static Fix64Quat FromTwoVectors(Fix64Vec3 a, Fix64Vec3 b)
        { // From: http://lolengine.net/blog/2014/02/24/quaternion-from-two-vectors-final
            Fix64 epsilon = Fix64.FromDivision(1, 1000000);
            Fix64 ab = Fix64Vec3.LengthSqr(a) * Fix64Vec3.LengthSqr(b);
            Fix64 norm_a_norm_b = Fix64.FromRaw(NativeFixedMath.Sqrt64(ab.Raw));
            Fix64 real_part = norm_a_norm_b + Fix64Vec3.Dot(a, b);

            Fix64Vec3 v;

            if (real_part < (epsilon * norm_a_norm_b))
            {
                /* If u and v are exactly opposite, rotate 180 degrees
                 * around an arbitrary orthogonal axis. Axis normalization
                 * can happen later, when we normalize the quaternion. */
                real_part = Fix64.zero;
                bool cond = NativeFixedMath.Abs64(a.x.Raw) > NativeFixedMath.Abs64(a.z.Raw);

                v = cond ? new Fix64Vec3(-a.y, a.x, Fix64.zero)
                         : new Fix64Vec3(Fix64.zero, -a.z, a.y);
            }
            else
            {
                /* Otherwise, build quaternion the standard way. */
                v = Fix64Vec3.Cross(a, b);
            }

            return Normalize(new Fix64Quat(v.x, v.y, v.z, real_part));
        }

        public static Fix64Quat LookRotation(Fix64Vec3 dir, Fix64Vec3 up)
        { // From: https://answers.unity.com/questions/819699/calculate-quaternionlookrotation-manually.html
            if (dir == Fix64Vec3.zero)
                return identity;

            if (up != dir)
            {
                Fix64Vec3 v = dir + up * -Fix64Vec3.Dot(up, dir);
                Fix64Quat q = FromTwoVectors(Fix64Vec3.forward, v);
                return FromTwoVectors(v, dir) * q;
            }
            else
                return FromTwoVectors(Fix64Vec3.forward, dir);
        }


        public static Fix64Quat Slerp(Fix64Quat q1, Fix64Quat q2, Fix64 t)
        {
            Fix64 epsilon = Fix64.FromDivision(1, 1000000);
            Fix64 cos_omega = q1.x * q2.x + q1.y * q2.y + q1.z * q2.z + q1.w * q2.w;

            bool flip = false;

            if (cos_omega < Fix64.zero)
            {
                flip = true;
                cos_omega = -cos_omega;
            }

            Fix64 s1, s2;
            if (cos_omega > (Fix64.one - epsilon))
            {
                // Too close, do straight linear interpolation.
                s1 = Fix64.one - t;
                s2 = (flip) ? -t : t;
            }
            else
            {
                Fix64 omega = Fix64.FromRaw(NativeFixedMath.Acos64(cos_omega.Raw));

                Fix64 inv_sin_omega = Fix64.one / Fix64.FromRaw(NativeFixedMath.Sin64(omega.Raw));

                Fix64 v1 = (Fix64.one - t) * omega;
                Fix64 v2 = t * omega;

                s1 = Fix64.FromRaw(NativeFixedMath.Sin64(v1.Raw)) * inv_sin_omega;
                s2 = Fix64.FromRaw(NativeFixedMath.Sin64(v2.Raw)) * inv_sin_omega;
                s2 = (flip) ? -s2 : s2;
            }

            return new Fix64Quat(
                s1 * q1.x + s2 * q2.x,
                s1 * q1.y + s2 * q2.y,
                s1 * q1.z + s2 * q2.z,
                s1 * q1.w + s2 * q2.w);
        }

        public override string ToString()
        {
            return $"Fix64Quat({x}, {y}, {z}, {w})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Fix64Quat))
                return false;
            return ((Fix64Quat)obj) == this;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() * 7919) ^ (z.GetHashCode() * 4513) ^ (w.GetHashCode() * 8923);
        }
    }
}