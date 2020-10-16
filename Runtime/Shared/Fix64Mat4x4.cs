using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    //column-major order.
    public struct Fix64Mat4x4
    {
        public static Fix64Mat4x4 identity
        {
            get
            {
                Fix64Vec4 _x = new Fix64Vec4(Fix64.one, Fix64.zero, Fix64.zero, Fix64.zero);
                Fix64Vec4 _y = new Fix64Vec4(Fix64.zero, Fix64.one, Fix64.zero, Fix64.zero);
                Fix64Vec4 _z = new Fix64Vec4(Fix64.zero, Fix64.zero, Fix64.one, Fix64.zero);
                Fix64Vec4 _w = new Fix64Vec4(Fix64.zero, Fix64.zero, Fix64.zero, Fix64.one);

                return new Fix64Mat4x4(_x, _y, _z, _w);
            }
        }

        public Fix64Vec4 x;
        public Fix64Vec4 y;
        public Fix64Vec4 z;
        public Fix64Vec4 w;

        public Fix64Mat4x4(Fix64Vec4 _x, Fix64Vec4 _y, Fix64Vec4 _z, Fix64Vec4 _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        //https://stackoverflow.com/a/51586282/1368748
        public static Fix64Mat4x4 FromTRS(Fix64Vec3 translation, Fix64Quat rotation, Fix64Vec3 scale)
        {
            Fix64 x1 = translation.x;
            Fix64 y1 = translation.y;
            Fix64 z1 = translation.z;
            Fix64 x2 = scale.x;
            Fix64 y2 = scale.y;
            Fix64 z2 = scale.z;

            Fix64Mat3x3 rot = new Fix64Mat3x3(rotation);
            Fix64 a11 = rot.x.x;
            Fix64 a21 = rot.x.y;
            Fix64 a31 = rot.x.z;
            Fix64 a12 = rot.y.x;
            Fix64 a22 = rot.y.y;
            Fix64 a32 = rot.y.z;
            Fix64 a13 = rot.z.x;
            Fix64 a23 = rot.z.y;
            Fix64 a33 = rot.z.z;


            Fix64Vec4 _x = new Fix64Vec4(x2 * a11, x2 * a21, x2 * a31, Fix64.zero);
            Fix64Vec4 _y = new Fix64Vec4(y2 * a12, y2 * a22, y2 * a32, Fix64.zero);
            Fix64Vec4 _z = new Fix64Vec4(z2 * a13, z2 * a23, z2 * a33, Fix64.zero);
            Fix64Vec4 _w = new Fix64Vec4(x1, y1, z1, Fix64.one);

            return new Fix64Mat4x4(_x, _y, _z, _w);
        }

        public static Fix64Vec4 operator *(Fix64Mat4x4 a, Fix64Vec4 v)
        {
            Fix64Vec4 r = v.x * a.x + v.y * a.y + v.z * a.z + v.w * a.w;
            return r;
        }

        public static Fix64Mat4x4 operator *(Fix64Mat4x4 a, Fix64Mat4x4 b)
        {
            Fix64Vec4 _x = a * b.x;
            Fix64Vec4 _y = a * b.y;
            Fix64Vec4 _z = a * b.z;
            Fix64Vec4 _w = a * b.w;

            return new Fix64Mat4x4(_x, _y, _z, _w);
        }


        public Fix64Vec3 MultiplyVector(Fix64Vec3 v)
        {
            Fix64 m00 = x.x, m01 = y.x, m02 = z.x;
            Fix64 m10 = x.y, m11 = y.y, m12 = z.y;
            Fix64 m20 = x.z, m21 = y.z, m22 = z.z;
            //Fix64 m30 = x.w, m31 = y.w, m32 = z.w;

            Fix64 _x = v.x * m00 + v.y * m01 + v.z * m02;
            Fix64 _y = v.x * m10 + v.y * m11 + v.z * m12;
            Fix64 _z = v.x * m20 + v.y * m21 + v.z * m22;

            return new Fix64Vec3(_x, _y, _z);
        }

        public Fix64Vec3 MultiplyPoint3x4(Fix64Vec3 v)
        {
            Fix64 m00 = x.x, m01 = y.x, m02 = z.x, m03 = w.x;
            Fix64 m10 = x.y, m11 = y.y, m12 = z.y, m13 = w.y;
            Fix64 m20 = x.z, m21 = y.z, m22 = z.z, m23 = w.z;
            //Fix64 m30 = x.w, m31 = y.w, m32 = z.w, m33 = w.w;

            Fix64 _x = v.x * m00 + v.y * m01 + v.z * m02 + m03;
            Fix64 _y = v.x * m10 + v.y * m11 + v.z * m12 + m13;
            Fix64 _z = v.x * m20 + v.y * m21 + v.z * m22 + m23;

            return new Fix64Vec3(_x, _y, _z);
        }

        public override string ToString()
        {
            return
                $"{x.x}, {y.x}, {z.x}, {w.x}\n" +
                $"{x.y}, {y.y}, {z.y}, {w.y}\n" +
                $"{x.z}, {y.z}, {z.z}, {w.z}\n" +
                $"{x.w}, {y.w}, {z.w}, {w.w}\n";
        }
    }
}