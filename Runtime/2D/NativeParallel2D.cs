using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Parallel
{
    internal class NativeParallel2D
    {
        // Name of the plugin when using [DllImport]
#if !UNITY_EDITOR && UNITY_IOS
		const string PLUGIN_NAME = "__Internal";
#else
        const string PLUGIN_NAME = "Parallel";
#endif
        internal static void Initialize()
        {
            RegisterDebugCallback(NativeParallelEventHandler.OnDebugCallback);
        }

        [DllImport(PLUGIN_NAME)]
        internal static extern void RegisterDebugCallback(debugCallback cb);

        //2D world
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateWorld(Fix64Vec2 gravity, ContactEnterCallBack enterCallback, ContactExitCallBack exitCallback);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr SetGravity(IntPtr worldHandle, Fix64Vec2 gravity);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroyWorld(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void Step(IntPtr worldHandle, Fix64 time, int velocityIterations, int positionIterations);

        [DllImport(PLUGIN_NAME)]
        internal static extern void FindContacts(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void PrepareExternalContactData();

        [DllImport(PLUGIN_NAME)]
        internal static extern void AddExternalContactData(PContactExport2D contactExport);

        //2D body
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateBody(IntPtr worldHandle, 
            int bodyType, 
            Fix64Vec2 position, 
            Fix64 angle,
            Fix64 linearDamping,
            Fix64 angularDamping,
            bool fixedRotation,
            Fix64 gravityScale,
            ref UInt16 bodyID);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyTransform(IntPtr bodyHandle, Fix64Vec2 position, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyVelocity(IntPtr bodyHandle, Fix64Vec2 linearVelocity, Fix64 angularVelocity);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyProperties(IntPtr bodyHandle,
            int bodyType,
            Fix64 linearDamping,
            Fix64 angularDamping,
            bool fixedRotation,
            Fix64 gravityScale);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyForce(IntPtr bodyHandle, Fix64Vec2 point, Fix64Vec2 force);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyForceToCenter(IntPtr bodyHandle, Fix64Vec2 force);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyTorque(IntPtr bodyHandle, Fix64 torque);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyLinearImpulse(IntPtr bodyHandle, Fix64Vec2 point, Fix64Vec2 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyLinearImpulseToCenter(IntPtr bodyHandle, Fix64Vec2 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyAngularImpulse(IntPtr bodyHandle, Fix64 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroyBody(IntPtr worldHandle, IntPtr bodyHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetTransform(IntPtr bodyHandle, ref Fix64Vec2 pos, ref Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetVelocity(IntPtr bodyHandle, ref Fix64Vec2 linearVelocity, ref Fix64 rz);

        //2D fixture
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr AddFixtureToBody(IntPtr bodyHandle, IntPtr shapeHandle, Fix64 density);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr GetShapeOfFixture(IntPtr fixtureHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetLayer(IntPtr fixtureHandle, int layer, int layerMask, bool refilter);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetFixtureProperties(IntPtr fixtureHandle, bool isTrigger, Fix64 friction, Fix64 bounciness);

        //2D shapes
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateCircle(Fix64 radius, Fix64Vec2 center);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateCircle(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64 radius, Fix64Vec2 center);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateBox(Fix64 width, Fix64 height, Fix64Vec2 center, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBox(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64 width, Fix64 height, Fix64Vec2 center, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateCapsule(Fix64Vec2 v1, Fix64Vec2 v2, Fix64 radius, Fix64Vec2 center, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateCapsule(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64Vec2 v1, Fix64Vec2 v2, Fix64 radius, Fix64Vec2 center, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreatePolygon(ref ParallelVec2List parallelVec2List, Fix64Vec2 center, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdatePolygon(IntPtr shapeHandle, IntPtr fixtureHandle, ref ParallelVec2List parallelVec2List, Fix64Vec2 center, Fix64 angle);

        //cast
        [DllImport(PLUGIN_NAME)]
        internal static extern bool RayCast(Fix64Vec2 point1, Fix64Vec2 point2, int mask, ref Fix64Vec2 point, ref Fix64Vec2 normal, out UInt16 bodyID, IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern bool CircleCast(Fix64Vec2 center, Fix64 radius, int mask, Fix64Vec2 translation, ref Fix64Vec2 point, ref Fix64Vec2 normal, ref Fix64 fraction, out UInt16 bodyID, IntPtr worldHandle);

        //overlap
        [DllImport(PLUGIN_NAME)]
        internal static extern bool CircleOverlap(IntPtr worldHandle, Fix64Vec2 center, Fix64 radius, int mask, UInt16[] bodyIDs, ref int count);

        //contact
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr GetContactList(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr ExportAndReturnNextContact(IntPtr contactHandle, ref PContactExport2D export);

        [DllImport(PLUGIN_NAME)]
        internal static extern int ExportContacts(IntPtr worldHandle, IntPtr[] contacts, PContactExport2D[] exports, int limit);

        [DllImport(PLUGIN_NAME)]
        internal static extern int GetContactDetail(IntPtr contactHandle, ref Fix64Vec2 point1, ref Fix64Vec2 point2, ref Fix64 penetration1, ref Fix64 penetration2, ref Fix64Vec2 normal);

        //convex
        [DllImport(PLUGIN_NAME)]
        internal static extern void ConvexHull2D(ref ParallelVec2List parallelVec2List, ref ParallelVec2List parallelVec2ListOut, int limit);

        //vector
        [DllImport(PLUGIN_NAME)]
        internal static extern void Vec2Normalize64(Fix64Vec2 a, ref Fix64Vec2 result);

        [DllImport(PLUGIN_NAME)]
        internal static extern void Vec2Length64(Fix64Vec2 a, ref Fix64 result);
    }
}