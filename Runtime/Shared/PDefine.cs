using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    public static class ParallelConstants
    {
        public const int SHAPE_OVERLAP_BODY_COUNT_2D = 16;
        public const int SHAPE_OVERLAP_BODY_COUNT_3D = 32;

        public const int MAX_CONTACT_COUNT_2D = 1024 * 5 * 2;
        public const int MAX_CONTACT_COUNT_3D = 1024 * 5 * 20;

        public const int MAX_BODY_CONTACT_COUNT_2D = 8;
        public const int MAX_BODY_CONTACT_COUNT_3D = 16;

        public static Fix64 SMALLEST_RAYCAST_RANGE = Fix64.FromDivision(1, 1000);
    }

    public enum LogLevel
    {
        Verbose = 0,
        Warning = 1,
        Error = 2
    }

    public enum BodyType
    {
        Static = 0,
        Kinematic = 1,
        Dynamic = 2
    }


    public enum ContactState
    {
        Default = 0,
        Enter = 1,
        Exit = 2,
        Inactive = 3,
        Active = 4,
    }

    internal delegate void debugCallback(string message);

    public static class NativeParallelEventHandler
    {
        public static LogLevel logLevel = LogLevel.Verbose;

        [MonoPInvokeCallback(typeof(debugCallback))]
        public static void OnDebugCallback(string message)
        {
            if(logLevel > 0)
            {
                return;
            }
            Debug.LogWarning(message);
        }
    }

    //internal delegate void debugCallback(IntPtr request, int level, int size);

    //public static class NativeParallelEventHandler
    //{
    //    public static LogLevel logLevel = LogLevel.Verbose;

    //    [MonoPInvokeCallback(typeof(debugCallback))]
    //    public static void OnDebugCallback(IntPtr request, int level, int size)
    //    {
    //        if ((int)logLevel > level)
    //        {
    //            return;
    //        }

    //        //Ptr to string
    //        string debug_string = Marshal.PtrToStringAnsi(request, size);

    //        if (level == 2)
    //        {
    //            Debug.LogError(debug_string);
    //        }
    //        else if (level == 1)
    //        {
    //            Debug.LogWarning(debug_string);
    //        }
    //        else
    //        {
    //            Debug.Log(debug_string);
    //        }
    //    }
    //}
}