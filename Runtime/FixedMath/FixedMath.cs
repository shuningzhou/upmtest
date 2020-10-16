using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Parallel
{
    internal class NativeFixedMath
    {
        // Name of the plugin when using [DllImport]
#if !UNITY_EDITOR && UNITY_IOS
		const string PLUGIN_NAME = "__Internal";
#else
        const string PLUGIN_NAME = "fixedMath";
#endif
        //fixed point
        [DllImport(PLUGIN_NAME)]
        internal static extern long Rcp64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Sign64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Sqrt64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Abs64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Mul64(long a, long b);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Div64(long a, long b);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Atan264(long a, long b);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Asin64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Sin64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Cos64(long a);

        [DllImport(PLUGIN_NAME)]
        internal static extern long Acos64(long a);
    }
}