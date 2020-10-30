using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Parallel
{
    [Serializable]
    public struct ParallelEdge
    {
        public UInt32 origin;
        public UInt32 twin;
        public UInt32 face;
        public UInt32 prev;
        public UInt32 next;
    }

    [Serializable]
    public struct ParallelFace
    {
        public UInt32 edge;
    }

    [Serializable]
    public struct ParallelPlane
    {
        public Fix64Vec3 normal;
        public Fix64 offset;
    }

    [Serializable]
    public struct ParallelQHullData
    {
        public UInt32 vertexCount;
        public Fix64Vec3[] vertices;
        public UInt32 edgeCount;
        public ParallelEdge[] edges;
        public UInt32 faceCount;
        public ParallelFace[] faces;
        public ParallelPlane[] planes;
    }

    [Serializable]
    public struct ParallelQHullData2
    {
        public UInt32 triCount;
        public ParallelIntTriangle[] tris;
        public Vector3[] vertices;
    }

    [Serializable]
    public struct ParallelTriangle
    {
        public UInt32 v1;
        public UInt32 v2;
        public UInt32 v3;
    }

    [Serializable]
    public struct ParallelIntTriangle
    {
        public Int32 v1;
        public Int32 v2;
        public Int32 v3;
    }

    [Serializable]
    public struct ParallelMeshData
    {
        public UInt32 vertexCount;
        public Fix64Vec3[] vertices;
        public UInt32 triangleCount;
        public ParallelTriangle[] triangles;
    }

    public struct PRaycastHit3D
    {
        public IParallelRigidbody3D rigidbody;
        public Fix64Vec3 point;
        public Fix64Vec3 normal;
        public Fix64 fraction;
    }

    public struct PShapecastHit3D
    {
        public IParallelRigidbody3D rigidbody;
        public Fix64Vec3 point;
        public Fix64Vec3 normal;
        public Fix64 fraction;
    }

    public class PShapeOverlapResult3D
    {
        public IParallelRigidbody3D[] rigidbodies;
        public int count;

        public PShapeOverlapResult3D()
        {
            count = 0;
            rigidbodies = new IParallelRigidbody3D[ParallelConstants.SHAPE_OVERLAP_BODY_COUNT_3D];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PContactExport3D
    {
        public UInt32 id;
        public Fix64Vec3 relativeVelocity;
        public bool isTrigger;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PConvexCacheExport3D
    {
        public bool initialized;

        public Fix64 metric; // lenght or area or volume
        public UInt16 count; // number of support vertices

        // support vertices on proxy 1
        public byte index10;
        public byte index11;
        public byte index12;
        public byte index13;

        // support vertices on proxy 2
        public byte index20;
        public byte index21;
        public byte index22;
        public byte index23;

        public byte state; // sat result
        public byte type; // feature pair type
        public UInt32 findex1; // feature index on hull 1
        public UInt32 findex2; // feature index on hull 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PManifoldExport3D
    {
        public UInt32 pointCount;
        public Fix64Vec2 tangentImpulse;
        public Fix64 motorImpulse;

        public UInt32 p1triangleKey;
        public UInt64 p1key1;
        public UInt64 p1key2;
        public Fix64 p1normalImpulse;

        public UInt32 p2triangleKey;
        public UInt64 p2key1;
        public UInt64 p2key2;
        public Fix64 p2normalImpulse;

        public UInt32 p3triangleKey;
        public UInt64 p3key1;
        public UInt64 p3key2;
        public Fix64 p3normalImpulse;

        public UInt32 p4triangleKey;
        public UInt64 p4key1;
        public UInt64 p4key2;
        public Fix64 p4normalImpulse;
    }

    public class PContact3DWrapper
    {
        public PContact3D contact;
        public PContact3DWrapper next;
    }

    public class PInternalState3D
    {
        public PContactExport3D[] contactExports;
        public PConvexCacheExport3D[] convexExports;
        public PManifoldExport3D[] manifoldExports;
        public int contactCount;

        public PInternalState3D()
        {
            contactCount = 0;
            contactExports = new PContactExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D];
            convexExports = new PConvexCacheExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D];
            manifoldExports = new PManifoldExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D * 3];
        }
    }

    internal delegate void ContactEnterCallBack3D(IntPtr contactPtr, UInt32 contactID);
    internal delegate void ContactExitCallBack3D(IntPtr contactPtr, UInt32 contactID);

    public class Parallel3D
    {
        static SortedList<UInt32, PBody3D> bodySortedList = new SortedList<UInt32, PBody3D>();

        //CONTACT
        static int _contactCount;
        static IntPtr[] contactPtrs = new IntPtr[ParallelConstants.MAX_CONTACT_COUNT_3D];
        static PContactExport3D[] contactExports = new PContactExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D];
        static PConvexCacheExport3D[] convexExports = new PConvexCacheExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D];
        static PManifoldExport3D[] manifoldExports = new PManifoldExport3D[ParallelConstants.MAX_CONTACT_COUNT_3D * 3];

        static Dictionary<UInt32, PContact3D> contactDictionary = new Dictionary<uint, PContact3D>();
        static PCollision3D _tempCollision = new PCollision3D();

        //enter contacts
        static int _enterContactCount;
        static PContact3DWrapper _enterContactWrapperHead = new PContact3DWrapper();
        static PContact3DWrapper _enterContactWrapperEnd = _enterContactWrapperHead;
        //exit contacts
        static int _exitContactCount;
        static PContact3DWrapper _exitContactWrapperHead = new PContact3DWrapper();
        static PContact3DWrapper _exitContactWrapperEnd = _exitContactWrapperHead;
        //enter+stay contacts
        //we always export all contacts after each step update
        //we should loop through all the contacts
        //send OnCollisionEnter for the entering contacts
        //send OnCollisionStay to the rest of the contacts
        static int _allContactCount;
        static PContact3DWrapper _allContactWrapperHead = new PContact3DWrapper();
        static PContact3DWrapper _allContactWrapperEnd = _allContactWrapperHead;

        //
        static bool initialized = false;
        static PWorld3D internalWorld;
        public static Fix64Vec3 gravity = new Fix64Vec3(Fix64.zero, Fix64.FromDivision(-98, 10), Fix64.zero);
        public static bool allowSleep = true;
        public static bool warmStart = true;

        //body exports
        public static UInt16 bodyExportSize = 128;

        //used for cast and overlap queruies
        static UInt16[] _queryBodyIDs = new UInt16[ParallelConstants.SHAPE_OVERLAP_BODY_COUNT_3D];

        //layer
        static Dictionary<int, int> masksByLayer = new Dictionary<int, int>();

        //common
        public static void Initialize()
        {
            ReadCollisionLayerMatrix();
            NativeParallel3D.Initialize();
            internalWorld = CreateWorld(gravity, allowSleep, warmStart);

            for (int i = 0; i < ParallelConstants.MAX_CONTACT_COUNT_3D; i++)
            {
                _enterContactWrapperEnd.next = new PContact3DWrapper();
                _enterContactWrapperEnd = _enterContactWrapperEnd.next;

                _exitContactWrapperEnd.next = new PContact3DWrapper();
                _exitContactWrapperEnd = _exitContactWrapperEnd.next;

                _allContactWrapperEnd.next = new PContact3DWrapper();
                _allContactWrapperEnd = _allContactWrapperEnd.next;
            }

            initialized = true;
        }

        public static void CleanUp()
        {
            if (initialized)
            {
                masksByLayer.Clear();
                bodySortedList.Clear();
                DestroyWorld(internalWorld);
                initialized = false;
            }
        }

        public static void UpdateContacts()
        {
            NativeParallel3D.FindContacts(internalWorld.IntPointer);
        }

        public static void ExportEngineInternalState(PInternalState3D internalState)
        {
            internalState.contactCount = _contactCount;
            Array.Copy(contactExports, internalState.contactExports, _contactCount);
            Array.Copy(convexExports, internalState.convexExports, _contactCount);
            Array.Copy(manifoldExports, internalState.manifoldExports, _contactCount * 3);
        }

        public static void PrepareExternalContactData()
        {
            NativeParallel3D.PrepareExternalContactData();
        }

        public static void AddExternalContactWarmStartData(PInternalState3D state)
        {
            for(int i = 0; i < state.contactCount; i++)
            {
                PContactExport3D export = state.contactExports[i];
                int manifoldIndex = 3 * i;
                PManifoldExport3D m1 = state.manifoldExports[manifoldIndex];
                PManifoldExport3D m2 = state.manifoldExports[manifoldIndex + 1];
                PManifoldExport3D m3 = state.manifoldExports[manifoldIndex + 2];

                //NativeParallel3D.AddExternalContactWarmStartData(export.id, export.flag, (byte)export.manifoldCount, m1, m2, m3);
            }
        }

        public static void PrepareExternalConvexCacheData()
        {
            NativeParallel3D.PrepareExternalConvexCacheData();
        }
        
        public static void AddExternalConvexCache(PInternalState3D state)
        {
            for (int i = 0; i < state.contactCount; i++)
            {
                PContactExport3D export = state.contactExports[i];
                PConvexCacheExport3D convexExport = state.convexExports[i];

                NativeParallel3D.AddExternalConvexCacheData(export.id, convexExport);
            }
        }

        public static void ReadCollisionLayerMatrix()
        {
            for (int i = 0; i < 32; i++)
            {
                int mask = 0;
                for (int j = 0; j < 32; j++)
                {
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        mask |= 1 << j;
                    }
                }
                masksByLayer.Add(i, mask);
            }
        }

        public static void SetLoggingLevel(LogLevel level)
        {
            NativeParallelEventHandler.logLevel = level;
            if (!initialized)
            {
                Initialize();
            }
        }

        static void ExportFromEngine()
        {
            //using (new SProfiler($"ExportContacts"))
            {
                ExportContacts();
                PrepareContacts();
            }

            //using (new SProfiler($"rigidBody"))
            foreach(var pair in bodySortedList)
            {
                PBody3D body = pair.Value;
                body.ReadNative();
            }
        }

        public static void ExcuteUserCallbacks(Fix64 timeStep)
        {
            //call contact exit callback

            PContact3DWrapper currentWrapper = _exitContactWrapperHead;

            for (int i = 0; i < _exitContactCount; i++)
            {
                PContact3D contact = currentWrapper.contact;

                PBody3D body1 = bodySortedList[contact.Body1ID];
                PBody3D body2 = bodySortedList[contact.Body2ID];

                if (contact.IsTrigger)
                {
                    body1.RigidBody.OnParallelTriggerExit(body2.RigidBody);
                    body2.RigidBody.OnParallelTriggerExit(body1.RigidBody);
                }
                else
                {
                    _tempCollision.SetContact(contact, body2.RigidBody);
                    body1.RigidBody.OnParallelCollisionExit(_tempCollision);

                    _tempCollision.SetContact(contact, body1.RigidBody);
                    body2.RigidBody.OnParallelCollisionExit(_tempCollision);
                }

                contact.state = ContactState.Inactive;
                currentWrapper = currentWrapper.next;
            }

            //call contact stay callback
            currentWrapper = _allContactWrapperHead;

            for (int i = 0; i < _allContactCount; i++)
            {
                PContact3D contact = currentWrapper.contact;

                if (contact.state == ContactState.Active)
                {
                    PBody3D body1 = bodySortedList[contact.Body1ID];
                    PBody3D body2 = bodySortedList[contact.Body2ID];

                    if (contact.IsTrigger)
                    {
                        body1.RigidBody.OnParallelTriggerStay(body2.RigidBody);
                        body2.RigidBody.OnParallelTriggerStay(body1.RigidBody);
                    }
                    else
                    {
                        _tempCollision.SetContact(contact, body2.RigidBody);
                        body1.RigidBody.OnParallelCollisionStay(_tempCollision);

                        _tempCollision.SetContact(contact, body1.RigidBody);
                        body2.RigidBody.OnParallelCollisionStay(_tempCollision);
                    }
                }

                currentWrapper = currentWrapper.next;
            }

            //call contact enter callback
            currentWrapper = _enterContactWrapperHead;

            for (int i = 0; i < _enterContactCount; i++)
            {
                PContact3D contact = currentWrapper.contact;
                PBody3D body1 = bodySortedList[contact.Body1ID];
                PBody3D body2 = bodySortedList[contact.Body2ID];

                if (contact.IsTrigger)
                {
                    body1.RigidBody.OnParallelTriggerEnter(body2.RigidBody);
                    body2.RigidBody.OnParallelTriggerEnter(body1.RigidBody);
                }
                else
                {
                    _tempCollision.SetContact(contact, body2.RigidBody);
                    body1.RigidBody.OnParallelCollisionEnter(_tempCollision);

                    _tempCollision.SetContact(contact, body1.RigidBody);
                    body2.RigidBody.OnParallelCollisionEnter(_tempCollision);
                }

                contact.state = ContactState.Active;
                currentWrapper = currentWrapper.next;
            }
        }

        public static void ExcuteUserFixedUpdate(Fix64 time)
        {
            foreach(var pair in bodySortedList)
            {
                PBody3D body = pair.Value;
                body.Step(time);
            }
        }

        //3D
        static PWorld3D CreateWorld(Fix64Vec3 gravity, bool allowSleep, bool warmStart)
        {
            IntPtr m_NativeObject = NativeParallel3D.CreateWorld(gravity, allowSleep, warmStart, OnContactEnterCallback, OnContactExitCallBack);
            return new PWorld3D(m_NativeObject);
        }

        public static void GetWorldSize(ref Fix64Vec3 lower, ref Fix64Vec3 uppder)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.GetWorldSize(internalWorld.IntPointer, ref lower, ref uppder);
        }

        static void DestroyWorld(PWorld3D world)
        {
            NativeParallel3D.DestroyWorld(world.IntPointer);
        }

        public static PSnapshot3D Snapshot()
        {
            IntPtr m_NativeObject = NativeParallel3D.Snapshot(internalWorld.IntPointer);
            return new PSnapshot3D(m_NativeObject);
        }

        public static void Restore(PSnapshot3D snapshot)
        {
            NativeParallel3D.Restore(internalWorld.IntPointer, snapshot.IntPointer);

            foreach (var pair in bodySortedList)
            {
                PBody3D body = pair.Value;
                body.ReadNative();
            }
        }

        public static void DestroySnapshot(PSnapshot3D snapshot)
        {
            NativeParallel2D.DestroySnapshot(snapshot.IntPointer);
        }

        public static void Step(Fix64 time, int velocityIterations, int positionIterations)
        {
            if (!initialized)
            {
                Initialize();
            }

            //using (new SProfiler($"==========STEP========"))
            {
                ResetEnterContacts();
                ResetExitContacts();
                ResetAllContacts();

                //using (new SProfiler($"Physics"))
                {
                    NativeParallel3D.Step(internalWorld.IntPointer, time, velocityIterations, positionIterations);
                }

                //using (new SProfiler($"Export"))
                {
                    ExportFromEngine();
                }
            }

        }

        //3D fixture
        public static PFixture3D AddFixture(PBody3D body, PShape3D shape, Fix64 density)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.AddFixtureToBody(body.IntPointer, shape.IntPointer, density);
            return new PFixture3D(m_NativeObject);
        }

        public static PShape3D GetShapeOfFixture(PFixture3D fixture)
        {
            if (!initialized)
            {
                Initialize();
            }

            return new PShape3D(fixture.IntPointer);
        }

        public static void SetLayer(PFixture3D fixture, int layer, bool refilter)
        {
            if (!initialized)
            {
                Initialize();
            }

            int mask = masksByLayer[layer];
            //shift layer
            int shiftedLayer = 1 << layer;

            NativeParallel3D.SetLayer(fixture.IntPointer, shiftedLayer, mask, refilter);
        }

        public static void SetFixtureProperties(PFixture3D fixture, bool isTrigger, Fix64 friction, Fix64 bounciness)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.SetFixtureProperties(fixture.IntPointer, isTrigger, friction, bounciness);
        }

        //3D shapes
        public static PShape3D CreateCube(Fix64 x, Fix64 y, Fix64 z, Fix64Vec3 center, Fix64Quat rotation)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.CreateCube(x, y, z, center, rotation);
            return new PShape3D(m_NativeObject);
        }

        public static void UpdateCube(PShape3D shape, PFixture3D fixture, Fix64 x, Fix64 y, Fix64 z, Fix64Vec3 center, Fix64Quat rotation)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateCube(shape.IntPointer, fixture.IntPointer, x, y, z, center, rotation);
        }

        public static PShape3D CreateSphere(Fix64 radius, Fix64Vec3 center)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.CreateSphere(radius, center);
            return new PShape3D(m_NativeObject);
        }

        public static void UpdateSphere(PShape3D shape, PFixture3D fixture, Fix64 radius, Fix64Vec3 center)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateSphere(shape.IntPointer, fixture.IntPointer, radius, center);
        }

        public static PShape3D CreateCapsule(Fix64Vec3 v1, Fix64Vec3 v2, Fix64 radius, Fix64Vec3 center, Fix64Quat rotation)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.CreateCapsule(v1, v2, radius, center, rotation);
            return new PShape3D(m_NativeObject);
        }

        public static void UpdateCapsule(PShape3D shape, PFixture3D fixture, Fix64Vec3 v1, Fix64Vec3 v2, Fix64 radius, Fix64Vec3 center, Fix64Quat rotation)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateCapsule(shape.IntPointer, fixture.IntPointer, v1, v2, radius, center, rotation);
        }

        public static PShape3D CreatePolyhedron(ParallelQHullData parallelQHullData, Fix64Vec3 scale, Fix64Vec3 center, Fix64Quat rotation)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.CreateConvex(parallelQHullData.vertices, parallelQHullData.vertexCount, parallelQHullData.edges, parallelQHullData.edgeCount, parallelQHullData.faces, parallelQHullData.faceCount, parallelQHullData.planes, scale, center, rotation);
            return new PShape3D(m_NativeObject);
        }

        public static void UpdatePolyhedron(PShape3D shape, PFixture3D fixture, Fix64Vec3 scale)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateConvex(shape.IntPointer, fixture.IntPointer, scale);
        }

        public static PShape3D CreateMesh(ParallelMeshData parallelMeshData, Fix64Vec3 scale)
        {
            if (!initialized)
            {
                Initialize();
            }

            IntPtr m_NativeObject = NativeParallel3D.CreateMesh(parallelMeshData.vertices, parallelMeshData.vertexCount, parallelMeshData.triangles, parallelMeshData.triangleCount, scale);

            return new PShape3D(m_NativeObject);
        }

        public static void UpdateMesh(PShape3D shape, PFixture3D fixture, Fix64Vec3 scale)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateMesh(shape.IntPointer, fixture.IntPointer, scale);
        }

        //3D body
        public static PBody3D AddBody(
            int bodyType, 
            Fix64Vec3 position, 
            Fix64Quat orientation,
            Fix64Vec3 linearDamping,
            Fix64Vec3 angularDamping,
            Fix64Vec3 gravityScale,
            bool fixedRotationX,
            bool fixedRotationY,
            bool fixedRotationZ,
            IParallelRigidbody3D rigidBody)
        {
            if (!initialized)
            {
                Initialize();
            }

            UInt16 bodyID = 0;

            IntPtr m_NativeObject = NativeParallel3D.CreateBody(
                internalWorld.IntPointer, 
                bodyType, 
                position, 
                orientation, 
                linearDamping,
                angularDamping,
                gravityScale,
                fixedRotationX,
                fixedRotationY,
                fixedRotationZ,
                ref bodyID);

            PBody3D body = new PBody3D(m_NativeObject, bodyID, rigidBody as ParallelRigidbody3D, bodyExportSize);
            bodySortedList[bodyID] = body;

            ReadNativeBody(body);

            return body;
        }

        public static void UpdateBodyTransForm(PBody3D body, Fix64Vec3 pos, Fix64Quat rot)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateBodyTransform(body.IntPointer, pos, rot);
        }

        public static void UpdateBodyTransformForRollback(PBody3D body, Fix64Vec3 pos, Fix64Quat rot, Fix64Quat rot0)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateBodyTransformForRollback(body.IntPointer, pos, rot, rot0);
        }

        public static void UpdateBodyVelocity(PBody3D body, Fix64Vec3 linearVelocity, Fix64Vec3 angularVelocity)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateBodyVelocity(body.IntPointer, linearVelocity, angularVelocity);
        }

        public static void UpdateBodyProperties(PBody3D body,
            int bodyType,
            Fix64Vec3 linearDamping,
            Fix64Vec3 angularDamping,
            Fix64Vec3 gravityScale,
            bool fixedRotationX,
            bool fixedRotationY,
            bool fixedRotationZ)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateBodyProperties(
                body.IntPointer, 
                bodyType, 
                linearDamping, 
                angularDamping, 
                gravityScale,
                fixedRotationX,
                fixedRotationY,
                fixedRotationZ);
        }

        public static Fix64Vec3 GetPointVelocity(PBody3D body, Fix64Vec3 point)
        {
            if (!initialized)
            {
                Initialize();
            }

            Fix64Vec3 result = Fix64Vec3.zero;

            NativeParallel3D.GetPointVelocity(body.IntPointer, point, ref result);

            return result;
        }


        public static void UpdateMassData(PBody3D body, Fix64 mass, Fix64Vec3 centerOfMass)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateMassData(body.IntPointer, mass, centerOfMass);
        }

        public static void UpdateMass(PBody3D body, Fix64 mass)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.UpdateMass(body.IntPointer, mass);
        }

        public static void ApplyForce(PBody3D body, Fix64Vec3 point, Fix64Vec3 force)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyForce(body.IntPointer, point, force);
        }

        public static void ApplyForceToCenter(PBody3D body, Fix64Vec3 force)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyForceToCenter(body.IntPointer, force);
        }

        public static void ApplyTorque(PBody3D body, Fix64Vec3 torque)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyTorque(body.IntPointer, torque);
        }

        public static void ApplyLinearImpulse(PBody3D body, Fix64Vec3 point, Fix64Vec3 impulse)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyLinearImpulse(body.IntPointer, point, impulse);
        }

        public static void ApplyLinearImpulseToCenter(PBody3D body, Fix64Vec3 impulse)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyLinearImpulseToCenter(body.IntPointer, impulse);
        }

        public static void ApplyAngularImpulse(PBody3D body, Fix64Vec3 impulse)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ApplyAngularImpulse(body.IntPointer, impulse);
        }

        public static void DestoryBody(PBody3D body, IParallelRigidbody3D rigidBody3D)
        {
            if (!initialized)
            {
                Initialize();
            }

            if(bodySortedList.ContainsKey(body.BodyID))
            {
                bodySortedList.Remove(body.BodyID);
            }

            NativeParallel3D.DestroyBody(internalWorld.IntPointer, body.IntPointer);
        }

        public static void ReadNativeBody(PBody3D body)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.GetTransform(body.IntPointer, ref body.position, ref body.orientation, ref body.orientation0);
            NativeParallel3D.GetVelocity(body.IntPointer, ref body.linearVelocity, ref body.angularVelocity);
            body.awake = NativeParallel3D.IsAwake(body.IntPointer);
            NativeParallel3D.GetSleepTime(body.IntPointer, ref body.sleepTime);
        }

        public static void SetAwakeForRollback(PBody3D body, bool awake, Fix64 sleepTime)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.SetAwakeForRollback(body.IntPointer, awake, sleepTime);
        }

        public static void SetAwake(PBody3D body, bool awake)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.SetAwake(body.IntPointer, awake);
        }

        public static bool IsAwake(PBody3D body)
        {
            if (!initialized)
            {
                Initialize();
            }

            return NativeParallel3D.IsAwake(body.IntPointer);
        }

        public static ParallelQHullData ConvextHull3D(Fix64Vec3[] verts, UInt32 count, bool simplify, Fix64 rad)
        {
            if (!initialized)
            {
                Initialize();
            }

            UInt32 outCount = 1024 * 10;
            Fix64Vec3[] vertsOut = new Fix64Vec3[outCount];
            ParallelEdge[] edgesOut = new ParallelEdge[outCount];
            ParallelFace[] facesOut = new ParallelFace[outCount];
            ParallelPlane[] planesOut = new ParallelPlane[outCount];

            UInt32 vertsOutCount = outCount;
            UInt32 edgesOutCount = outCount;
            UInt32 facesOutCount = outCount;

            NativeParallel3D.ConvexHull3D(verts, count, vertsOut, ref vertsOutCount, edgesOut, ref edgesOutCount, facesOut, ref facesOutCount, planesOut, simplify, rad);

            ParallelQHullData parallelQHullData = new ParallelQHullData();
            parallelQHullData.vertexCount = vertsOutCount;
            parallelQHullData.edgeCount = edgesOutCount;
            parallelQHullData.faceCount = facesOutCount;
            parallelQHullData.vertices = vertsOut;
            parallelQHullData.edges = edgesOut;
            parallelQHullData.faces = facesOut;
            parallelQHullData.planes = planesOut;
            return parallelQHullData;
        }


        public static void ConvextHull3D1(Vector3[] verts, UInt32 count)
        {
            if (!initialized)
            {
                Initialize();
            }

            NativeParallel3D.ConvexHull3D1(verts, count);
        }

        public static ParallelQHullData2 ConvextHull3D2(Vector3[] verts, UInt32 count, int limit)
        {
            if (!initialized)
            {
                Initialize();
            }

            UInt32 outCount = 1024;
            ParallelIntTriangle[] trisOut = new ParallelIntTriangle[outCount];

            Vector3[] vertsOut = new Vector3[count];

            UInt32 trisOutCount = outCount;

            NativeParallel3D.ConvexHull3D2(verts, count, trisOut, ref trisOutCount, vertsOut, limit);

            ParallelQHullData2 parallelQHullData = new ParallelQHullData2();
            parallelQHullData.triCount = trisOutCount;
            parallelQHullData.tris = trisOut;
            parallelQHullData.vertices = vertsOut;
            return parallelQHullData;
        }

        //raycast
        public static bool RayCast(Fix64Vec3 start, Fix64Vec3 direction, Fix64 range, out PRaycastHit3D raycastHit3D)
        {
            return RayCast(start, start + range * direction, out raycastHit3D);
        }

        public static bool RayCast(Fix64Vec3 start, Fix64Vec3 end, out PRaycastHit3D raycastHit3D)
        {
            return RayCast(start, end, -1, out raycastHit3D);
        }

        public static bool RayCast(Fix64Vec3 start, Fix64Vec3 direction, Fix64 range, int mask, out PRaycastHit3D raycastHit3D)
        {
            return RayCast(start, start + range * direction, mask, out raycastHit3D);
        }

        public static bool RayCast(Fix64Vec3 start, Fix64Vec3 end, int mask, out PRaycastHit3D raycastHit3D)
        {
            if (!initialized)
            {
                Initialize();
            }

            raycastHit3D = new PRaycastHit3D();

            if (Fix64Vec3.Distance(start, end) < ParallelConstants.SMALLEST_RAYCAST_RANGE)
            {
                Debug.Log("RayCast range too short");
                return false;
            }

            Fix64Vec3 point = Fix64Vec3.zero;
            Fix64Vec3 normal = Fix64Vec3.zero;
            Fix64 fraction = Fix64.one;

            raycastHit3D = new PRaycastHit3D();
            UInt16 bodyID = 0;

            bool hit = NativeParallel3D.RayCast(start, end, mask, ref point, ref normal, ref fraction, ref bodyID, internalWorld.IntPointer);

            if (hit)
            {
                raycastHit3D.point = point;
                raycastHit3D.normal = normal;
                raycastHit3D.fraction = fraction;
                if (bodySortedList.ContainsKey(bodyID))
                {
                    raycastHit3D.rigidbody = bodySortedList[bodyID].RigidBody;
                }
                else
                {
                    Debug.LogError($"Rigibody not found: {bodyID}");
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool SphereCast(Fix64Vec3 start, Fix64 radius, Fix64Vec3 movement, ref PShapecastHit3D shapeCastHit3D)
        {
            return SphereCast(start, radius, movement, -1, ref shapeCastHit3D);
        }

        public static bool SphereCast(Fix64Vec3 start, Fix64 radius, Fix64Vec3 movement, int mask, ref PShapecastHit3D shapeCastHit3D)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (movement.Length() < ParallelConstants.SMALLEST_RAYCAST_RANGE)
            {
                //Debug.Log("ShapeCast range too short");
                return false;
            }

            Fix64Vec3 point = Fix64Vec3.zero;
            Fix64Vec3 normal = Fix64Vec3.zero;
            Fix64 fraction = Fix64.one;
            UInt16 bodyID = 0;

            bool hit = NativeParallel3D.SphereCast(internalWorld.IntPointer, mask, start, radius, movement, ref point, ref normal, ref fraction, ref bodyID);

            if (hit)
            {
                shapeCastHit3D.point = point;
                shapeCastHit3D.normal = normal;
                shapeCastHit3D.fraction = fraction;

                if (bodySortedList.ContainsKey(bodyID))
                {
                    shapeCastHit3D.rigidbody = bodySortedList[bodyID].RigidBody;
                }
                else
                {
                    Debug.LogError($"Rigibody not found: {bodyID}");
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        //overlap
        public static bool OverlapSphere(Fix64Vec3 center, Fix64 radius, PShapeOverlapResult3D shapeOverlapResult)
        {
            return OverlapSphere(center, radius, -1, shapeOverlapResult);
        }

        public static bool OverlapSphere(Fix64Vec3 center, Fix64 radius, int mask, PShapeOverlapResult3D shapeOverlapResult)
        {
            if (!initialized)
            {
                Initialize();
            }

            int count = 0;
            bool hit = NativeParallel3D.SphereOverlap(internalWorld.IntPointer, mask, center, radius, _queryBodyIDs, ref count);

            shapeOverlapResult.count = count;

            for (int i = 0; i < count; i++)
            {
                UInt16 bodyID = _queryBodyIDs[i];
                if (bodySortedList.ContainsKey(bodyID))
                {
                    shapeOverlapResult.rigidbodies[i] = bodySortedList[bodyID].RigidBody;
                }
                else
                {
                    Debug.LogError($"Rigibody not found: {bodyID}");
                }
            }

            return hit;
        }


        public static bool OverlapCube(Fix64Vec3 center, Fix64Quat rotation, Fix64 x, Fix64 y, Fix64 z, int mask, PShapeOverlapResult3D shapeOverlapResult)
        {
            if (!initialized)
            {
                Initialize();
            }

            int count = 0;
            bool hit = NativeParallel3D.CubeOverlap(internalWorld.IntPointer, mask, center, rotation, x, y, z, _queryBodyIDs, ref count);

            shapeOverlapResult.count = count;

            for (int i = 0; i < count; i++)
            {
                UInt16 bodyID = _queryBodyIDs[i];
                if (bodySortedList.ContainsKey(bodyID))
                {
                    shapeOverlapResult.rigidbodies[i] = bodySortedList[bodyID].RigidBody;
                }
                else
                {
                    Debug.LogError($"Rigibody not found: {bodyID}");
                }
            }

            return hit;
        }

        //contact
        [MonoPInvokeCallback(typeof(ContactEnterCallBack3D))]
        public static void OnContactEnterCallback(IntPtr contactPtr, UInt32 contactID)
        {
            PContact3D c;

            if (contactDictionary.ContainsKey(contactID))
            {
                //already has this contact
                //the native contact for this body pair was created and destroyed before
                c = contactDictionary[contactID];
                c.state = ContactState.Enter;

            }
            else
            {
                //first time
                c = new PContact3D(contactID);
                c.state = ContactState.Enter;
                contactDictionary[contactID] = c;
            }

            AddEnterContactWrapper(c);
            //Debug.Log("Enter contact");
        }

        static void AddEnterContactWrapper(PContact3D contact)
        {
            _enterContactWrapperEnd.contact = contact;

            _enterContactWrapperEnd = _enterContactWrapperEnd.next;

            _enterContactCount++;
        }

        static void ResetEnterContacts()
        {
            _enterContactCount = 0;
            _enterContactWrapperEnd = _enterContactWrapperHead;
        }

        [MonoPInvokeCallback(typeof(ContactExitCallBack3D))]
        public static void OnContactExitCallBack(IntPtr contactPtr, UInt32 contactID)
        {
            PContact3D c;

            if (contactDictionary.ContainsKey(contactID))
            {
                //already has this contact
                //the native contact for this body pair was created and destroyed before
                c = contactDictionary[contactID];
                c.state = ContactState.Exit;

            }
            else
            {
                //first time
                c = new PContact3D(contactID);
                c.state = ContactState.Enter;
                contactDictionary[contactID] = c;
            }

            AddExitContactWrapper(c);
            //Debug.Log("Exit contact");
        }

        static void AddExitContactWrapper(PContact3D contact)
        {
            _exitContactWrapperEnd.contact = contact;

            _exitContactWrapperEnd = _exitContactWrapperEnd.next;

            _exitContactCount++;
        }

        static void ResetExitContacts()
        {
            _exitContactCount = 0;
            _exitContactWrapperEnd = _exitContactWrapperHead;
        }

        public static void PrepareContacts()
        {
            for (int i = 0; i < _contactCount; i++)
            {
                PContactExport3D export = contactExports[i];

                if (export.id == 0)
                {
                    continue;
                }

                PContact3D c;

                if (contactDictionary.ContainsKey(export.id))
                {
                    c = contactDictionary[export.id];
                }
                else
                {
                    c = new PContact3D(export.id);
                    contactDictionary[export.id] = c;
                }

                c.Update(
                    contactPtrs[i],
                    export.relativeVelocity,
                    export.isTrigger
                    );

                AddAllContactWrapper(c);
            }
        }

        static void AddAllContactWrapper(PContact3D contact)
        {
            _allContactWrapperEnd.contact = contact;

            _allContactWrapperEnd = _allContactWrapperEnd.next;

            _allContactCount++;
        }

        static void ResetAllContacts()
        {
            _allContactCount = 0;
            _allContactWrapperEnd = _allContactWrapperHead;
        }

        public static void ExportContacts()
        {
            if (!initialized)
            {
                Initialize();
            }

            _contactCount = 0;
            int index = 0;

            IntPtr contactPtr = NativeParallel3D.GetContactList(internalWorld.IntPointer);

            while (contactPtr != IntPtr.Zero)
            {
                contactPtrs[index] = contactPtr;
                PContactExport3D export = contactExports[index];

                contactPtr = NativeParallel3D.ExportAndReturnNextContact(contactPtr, ref export);
                contactExports[index] = export;
                index++;
            }

            _contactCount = index;
        }

        //public static void GetContactDetail(IntPtr contactHandler, ref PContactPoints2D contactPoints2D)
        //{
        //    if (!initialized)
        //    {
        //        Initialize();
        //    }

        //    contactPoints2D.contactPointCount = 0;


        //    contactPoints2D.contactPointCount = NativeParallel.GetContactDetail(
        //                                                            contactHandler,
        //                                                            ref contactPoints2D.point1,
        //                                                            ref contactPoints2D.point2,
        //                                                            ref contactPoints2D.penetration1,
        //                                                            ref contactPoints2D.penetration2,
        //                                                            ref contactPoints2D.contactNormal);
        //}

        //triangulation
        public static PolyIsland CreatePolyIsland(Fix64Vec2[] verts, int[] indexes, int count)
        {
            IntPtr m_NativeObject = NativeParallel3D.CreatePolyIsland(verts, indexes, count);
            return new PolyIsland(m_NativeObject);
        }

        public static void DestroyPolyIsland(PolyIsland polyIsland)
        {
            NativeParallel3D.DestroyPolyIsland(polyIsland.IntPointer);
        }

        public static void AddHolePolyIsland(Fix64Vec2[] verts, int[] indexes, int count, PolyIsland polyIsland)
        {
            NativeParallel3D.AddHolePolyIsland(verts, indexes, count, polyIsland.IntPointer);
        }

        public static bool TriangulatePolyIsland(int[] indices, int[] indiceCounts, ref int triangleCount, ref int totalIndicesCount, int level, PolyIsland polyIsland)
        {
            return NativeParallel3D.TriangulatePolyIsland(indices, indiceCounts, ref triangleCount, ref totalIndicesCount, level, polyIsland.IntPointer);
        }
    }
}