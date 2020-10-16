using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    //column-major order.
    public struct Fix64Mat3x3
    {
        public Fix64Vec3 x;
        public Fix64Vec3 y;
        public Fix64Vec3 z;

        public Fix64Mat3x3(Fix64Quat quat)
        {
            Fix64 _x = quat.x;
            Fix64 _y = quat.y;
            Fix64 _z = quat.z;
            Fix64 _w = quat.w;

            Fix64 x2 = _x + _x, y2 = _y + _y, z2 = _z + _z;
            Fix64 xx = _x * x2, xy = _x * y2, xz = _x * z2;
            Fix64 yy = _y * y2, yz = _y * z2, zz = _z * z2;
            Fix64 wx = _w * x2, wy = _w * y2, wz = _w * z2;

            x = new Fix64Vec3(Fix64.one - (yy + zz), xy + wz, xz - wy);
			y = new Fix64Vec3(xy - wz, Fix64.one - (xx + zz), yz + wx);
			z = new Fix64Vec3(xz + wy, yz - wx, Fix64.one - (xx + yy));
        }
    }
}
