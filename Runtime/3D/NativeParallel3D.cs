using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;

namespace Parallel
{
    internal class NativeParallel3D
    {
        // Name of the plugin when using [DllImport]
#if !UNITY_EDITOR && UNITY_IOS
		const string PLUGIN_NAME = "__Internal";
#else
        const string PLUGIN_NAME = "Parallel3D";
#endif
        internal static void Initialize()
        {
            RegisterDebugCallback(NativeParallelEventHandler.OnDebugCallback);
        }

        [DllImport(PLUGIN_NAME)]
        internal static extern void RegisterDebugCallback(debugCallback cb);

        //3D world
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateWorld(Fix64Vec3 gravity, bool allowSleep, bool warmStart, ContactEnterCallBack3D enterCallback, ContactExitCallBack3D exitCallback);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetWorldSize(IntPtr worldHandle, ref Fix64Vec3 lower, ref Fix64Vec3 upper);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr SetGravity(IntPtr worldHandle, Fix64Vec3 gravity);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroyWorld(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void Step(IntPtr worldHandle, Fix64 time, int velocityIterations, int positionIterations);

        [DllImport(PLUGIN_NAME)]
        internal static extern void FindContacts(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void PrepareExternalContactData();

        [DllImport(PLUGIN_NAME)]
        internal static extern void PrepareExternalConvexCacheData();

        [DllImport(PLUGIN_NAME)]
        internal static extern void AddExternalContactWarmStartData(
                            UInt32 contactID,
                            UInt32 flag,
                            byte manifoldCount, 
                            PManifoldExport3D manifoldExport3D1,
                            PManifoldExport3D manifoldExport3D2,
                            PManifoldExport3D manifoldExport3D3);

        [DllImport(PLUGIN_NAME)]
        internal static extern void AddExternalConvexCacheData(
                    UInt32 contactID,
                    PConvexCacheExport3D convexExport);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr Snapshot(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void Restore(IntPtr worldHandle, IntPtr snapshotHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroySnapshot(IntPtr snapshotHandle);

        //3D body
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateBody(IntPtr worldHandle, 
            int bodyType, 
            Fix64Vec3 position, 
            Fix64Quat orientation,
            Fix64Vec3 linearDamping,
            Fix64Vec3 angularDamping,
            Fix64Vec3 gravityScale,
            bool fixedRotationX,
            bool fixedRotationY,
            bool fixedRotationZ,
            ref UInt16 bodyID);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyTransform(IntPtr bodyHandle, Fix64Vec3 position, Fix64Quat orientation);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyTransformForRollback(IntPtr bodyHandle, Fix64Vec3 position, Fix64Quat orientation, Fix64Quat orientation0);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyVelocity(IntPtr bodyHandle, Fix64Vec3 linearVelocity, Fix64Vec3 angularVelocity);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateBodyProperties(IntPtr bodyHandle,
            int bodyType,
            Fix64Vec3 linearDamping,
            Fix64Vec3 angularDamping,
            Fix64Vec3 gravityScale,
            bool fixedRotationX,
            bool fixedRotationY,
            bool fixedRotationZ);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyForce(IntPtr bodyHandle, Fix64Vec3 point, Fix64Vec3 force);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyForceToCenter(IntPtr bodyHandle, Fix64Vec3 force);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyTorque(IntPtr bodyHandle, Fix64Vec3 torque);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyLinearImpulse(IntPtr bodyHandle, Fix64Vec3 point, Fix64Vec3 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyLinearImpulseToCenter(IntPtr bodyHandle, Fix64Vec3 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ApplyAngularImpulse(IntPtr bodyHandle, Fix64Vec3 impulse);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroyBody(IntPtr worldHandle, IntPtr bodyHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetTransform(IntPtr bodyHandle, ref Fix64Vec3 pos, ref Fix64Quat orientation, ref Fix64Quat orientation0);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetVelocity(IntPtr bodyHandle, ref Fix64Vec3 linearVelocity, ref Fix64Vec3 rz);

        [DllImport(PLUGIN_NAME)]
        internal static extern bool IsAwake(IntPtr bodyHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetAwake(IntPtr bodyHandle, bool awake);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetSleepTime(IntPtr bodyHandle, ref Fix64 sleepTime);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetAwakeForRollback(IntPtr bodyHandle, bool awake, Fix64 sleepTime);

        [DllImport(PLUGIN_NAME)]
        internal static extern void GetPointVelocity(IntPtr bodyHandle, Fix64Vec3 point, ref Fix64Vec3 v);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateMassData(IntPtr bodyHandle, Fix64 mass, Fix64Vec3 centerOfMass);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateMass(IntPtr bodyHandle, Fix64 mass);

        //3D fixture
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr AddFixtureToBody(IntPtr bodyHandle, IntPtr shapeHandle, Fix64 density);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetLayer(IntPtr fixtureHandle, int layer, int layerMask, bool refilter);

        [DllImport(PLUGIN_NAME)]
        internal static extern void SetFixtureProperties(IntPtr fixtureHandle, bool isTrigger, Fix64 friction, Fix64 bounciness);

        //3D shapes
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateCube(Fix64 x, Fix64 y, Fix64 z, Fix64Vec3 center, Fix64Quat rotation);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateCube(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64 x, Fix64 y, Fix64 z, Fix64Vec3 center, Fix64Quat rotation);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateSphere(Fix64 radius, Fix64Vec3 center);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateSphere(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64 radius, Fix64Vec3 center);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateCapsule(Fix64Vec3 v1, Fix64Vec3 v2, Fix64 radius, Fix64Vec3 center, Fix64Quat rotation);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateCapsule(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64Vec3 v1, Fix64Vec3 v2, Fix64 radius, Fix64Vec3 center, Fix64Quat rotation);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateConvex(Fix64Vec3[] verts, UInt32 vertsCount, ParallelEdge[] edges, UInt32 edgesCount, ParallelFace[] faces, UInt32 faceCount, ParallelPlane[] planes, Fix64Vec3 scale, Fix64Vec3 center, Fix64Quat rotation);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateConvex(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64Vec3 scale);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreateMesh(Fix64Vec3[] verts, UInt32 vertsCount, ParallelTriangle[] triangles, UInt32 triangleCount, Fix64Vec3 scale);

        [DllImport(PLUGIN_NAME)]
        internal static extern void UpdateMesh(IntPtr shapeHandle, IntPtr fixtureHandle, Fix64Vec3 scale);

        //cast
        [DllImport(PLUGIN_NAME)]
        internal static extern bool RayCast(
                            Fix64Vec3 point1, 
                            Fix64Vec3 point2, 
                            int mask,
                            ref Fix64Vec3 point, 
                            ref Fix64Vec3 normal, 
                            ref Fix64 fraction, 
                            ref UInt16 bodyID, 
                            IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern bool SphereCast(
                            IntPtr worldHandle,
                            int mask,
                            Fix64Vec3 center,
                            Fix64 radius,
                            Fix64Vec3 t,
                            ref Fix64Vec3 point,
                            ref Fix64Vec3 normal,
                            ref Fix64 fraction,
                            ref UInt16 bodyID);

        //overlap
        [DllImport(PLUGIN_NAME)]
        internal static extern bool SphereOverlap(IntPtr worldHandle, int mask, Fix64Vec3 center, Fix64 radius, UInt16[] bodyIDs, ref int count);

        [DllImport(PLUGIN_NAME)]
        internal static extern bool CubeOverlap(IntPtr worldHandle, int mask, Fix64Vec3 center, Fix64Quat rot, Fix64 x, Fix64 y, Fix64 z, UInt16[] bodyIDs, ref int count);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr GetContactList(IntPtr worldHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr ExportAndReturnNextContact(
                            IntPtr contactHandle, 
                            ref PContactExport3D export);

        //convex
        [DllImport(PLUGIN_NAME)]
        internal static extern void ConvexHull3D(Fix64Vec3[] verts, UInt32 vertsCount, Fix64Vec3[] vertsOut, ref UInt32 vertsOutCount, ParallelEdge[] edgesOut, ref UInt32 edgesOutCount, ParallelFace[] faceOut, ref UInt32 facesOutCount, ParallelPlane[] planesOut, bool simplify, Fix64 angle);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ConvexHull3D1(Vector3[] verts, UInt32 vertsCount);

        [DllImport(PLUGIN_NAME)]
        internal static extern void ConvexHull3D2(Vector3[] verts, UInt32 vertsCount, ParallelIntTriangle[] outTri, ref UInt32 outTriCount, Vector3[] outVerts, int limit);

        //transform
        [DllImport(PLUGIN_NAME)]
        internal static extern Fix64Vec3 Mul(Fix64Vec3 pos, Fix64Quat rot, Fix64Vec3 point, ref Fix64Vec3 output);

        [DllImport(PLUGIN_NAME)]
        internal static extern Fix64Vec3 MulT(Fix64Vec3 pos, Fix64Quat rot, Fix64Vec3 point, ref Fix64Vec3 output);

        //vector
        [DllImport(PLUGIN_NAME)]
        internal static extern void Vec3Normalize64(Fix64Vec3 a, ref Fix64Vec3 result);

        [DllImport(PLUGIN_NAME)]
        internal static extern void Vec3Length64(Fix64Vec3 a, ref Fix64 result);

        //triangulation
        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr CreatePolyIsland(
                                        Fix64Vec2[] verts, 
                                        int[] indexes, 
                                        int vertsCount);

        [DllImport(PLUGIN_NAME)]
        internal static extern void DestroyPolyIsland(IntPtr PolyIslandHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern IntPtr AddHolePolyIsland(
                                        Fix64Vec2[] verts, 
                                        int[] indexes, 
                                        int vertsCount, 
                                        IntPtr PolyIslandHandle);

        [DllImport(PLUGIN_NAME)]
        internal static extern bool TriangulatePolyIsland(
                                        int[] indices, 
                                        int[] indiceCounts, 
                                        ref int triangleCount, 
                                        ref int totalIndicesCount, 
                                        int level, 
                                        IntPtr PolyIslandHandle);
    }
}