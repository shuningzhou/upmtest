using UnityEngine;
using ParallelUnity.DebugTools;
using System;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public class ParallelConvexCollider3D : ParallelCollider3D
    {
        MeshFilter meshFilter;
        Mesh mesh;
        public Vector3[] verts = new Vector3[0];
        public int vertsCount = 0;

        bool _simplified = false;
        public bool simplified = false;
        public float angle = 15;

        UInt32 _limit = 24;
        public UInt32 Limit = 24;

        public ParallelQHullData convexData;
        public ParallelQHullData2 convexData2;

        Fix64Vec3 _currentSize = Fix64Vec3.one;

        void BuildConvexData()
        {
            //stan hull
            Vector3[] vIn1 = new Vector3[vertsCount];
            for (int i = 0; i < vertsCount; i++)
            {
                vIn1[i] = verts[i];
            }

            ParallelQHullData2 qhullData2 = Parallel3D.ConvextHull3D2(vIn1, (UInt32)vertsCount, (int)_limit);

            convexData2 = qhullData2;

            ParallelIntTriangle[] t = new ParallelIntTriangle[convexData2.triCount];
            Array.Copy(convexData2.tris, 0, t, 0, convexData2.triCount);
            convexData2.tris = t;

            //new convex hull
            Fix64Vec3[] vIn = new Fix64Vec3[_limit];
            for (int i = 0; i < _limit; i++)
            {
                vIn[i] = (Fix64Vec3)convexData2.vertices[i];
            }

            float rad = angle * Mathf.Deg2Rad;

            ParallelQHullData qhullData = Parallel3D.ConvextHull3D(vIn, (UInt32)_limit, _simplified, (Fix64)rad);

            convexData = qhullData;

            Fix64Vec3[] v = new Fix64Vec3[convexData.vertexCount];
            Array.Copy(convexData.vertices, 0, v, 0, convexData.vertexCount);
            convexData.vertices = v;

            string output = "";
            output += $"b3Vec3 verts[{convexData.vertexCount}] = {{}};\n";
            //Debug.Log($"b3Vec3 verts[{convexData.vertexCount}] = {{}};");
            for (int i = 0; i < convexData.vertexCount; i++)
            {
                Vector3 vec3 = (Vector3)convexData.vertices[i];
                output += $"b3Vec3({vec3.x}, {vec3.y}, {vec3.z}),\n";
                //Debug.Log($"verts[{i}] = b3Vec3({vec3.x}, {vec3.y}, {vec3.z});");
            }
            //Debug.Log(output);

            ParallelEdge[] e = new ParallelEdge[convexData.edgeCount];
            Array.Copy(convexData.edges, 0, e, 0, convexData.edgeCount);
            convexData.edges = e;

            ParallelFace[] f = new ParallelFace[convexData.faceCount];
            Array.Copy(convexData.faces, 0, f, 0, convexData.faceCount);
            convexData.faces = f;

            ParallelPlane[] p = new ParallelPlane[convexData.faceCount];
            Array.Copy(convexData.planes, 0, p, 0, convexData.faceCount);
            convexData.planes = p;

            return;
        }

        void Reset()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            Mesh mesh = meshFilter.sharedMesh;

            verts = mesh.vertices;
            vertsCount = verts.Length;

            BuildConvexData();
        }

        void DrawStanHull()
        {
            for (int i = 0; i < convexData2.triCount; i++)
            {
                ParallelIntTriangle t = convexData2.tris[i];
                Vector3 v1 = convexData2.vertices[t.v1];
                Vector3 v2 = convexData2.vertices[t.v2];
                Vector3 v3 = convexData2.vertices[t.v3];

                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v2.x, v2.y, v2.z));
                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v3.x, v3.y, v3.z));
                Gizmos.DrawLine(transform.TransformPoint(v3.x, v3.y, v3.z), transform.TransformPoint(v2.x, v2.y, v2.z));
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            for (UInt32 i = 0; i < convexData.edgeCount; i += 2)
            {
                ParallelEdge edge1 = convexData.edges[i];
                ParallelEdge edge2 = convexData.edges[i + 1];

                Vector3 v1 = (Vector3)convexData.vertices[edge1.origin];
                Vector3 v2 = (Vector3)convexData.vertices[edge2.origin];

                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v2.x, v2.y, v2.z));
            }


            Gizmos.color = Color.yellow;
            foreach (Fix64Vec3 fv in convexData.vertices)
            {
                Vector3 v = (Vector3)fv;
                Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
            }
        }

        Fix64Vec3 CalculateSize()
        {
            Fix64Vec3 s = pTransform.localScale;

            if (s.x > Fix64.zero && s.y > Fix64.zero)
            {
                return s;
            }
            else
            {
                Debug.LogError("Invalid size");
                return Fix64Vec3.zero;
            }
        }

        void Update()
        {
            if (simplified != _simplified)
            {
                _simplified = simplified;
                BuildConvexData();
            }

            if (Limit != _limit)
            {
                _limit = Limit;
                BuildConvexData();
            }
        }

        public void UpdateVertsLimit(UInt32 newLimit)
        {
            Limit = newLimit;
            _limit = newLimit;
            BuildConvexData();
        }

        protected override void UpdateShape(GameObject root)
        {
            Fix64Vec3 s = CalculateSize();
            if (s == Fix64Vec3.zero)
            {
                return;
            }

            if (s == _currentSize)
            {
                return;
            }

            Fix64Vec3 scale = s / _currentSize;

            _currentSize = s;

            Parallel3D.UpdatePolyhedron(_shape, _fixture, scale);
        }

        public override PShape3D CreateShape(GameObject root)
        {
            Fix64Vec3 s = CalculateSize();

            if (s != Fix64Vec3.zero)
            {
                _currentSize = s;

                Fix64Vec3 center = Fix64Vec3.zero;
                Fix64Quat rotation = Fix64Quat.identity;

                if (gameObject != root)
                {
                    center = _pTransform.localPosition;
                    rotation = _pTransform.localRotation;
                }

                _shape = Parallel3D.CreatePolyhedron(convexData, s, center, rotation);

                if(createUnityPhysicsCollider)
                {
                    var collider = gameObject.AddComponent<MeshCollider>();
                    collider.convex = true;
                }
                return _shape;
            }
            else
            {
                return null;
            }
        }
    }
}

//namespace Parallel
//{
//    [RequireComponent(typeof(ParallelTransform))]
//    [ExecuteInEditMode]
//    public class PConvexCollider3D : MonoBehaviour, IParallelCollider3D
//    {
//        PShape3D _shape;
//        PFixture3D _fixture;

//        MeshFilter meshFilter;
//        Mesh mesh;

//        public Vector3 editorSize = Vector3.one;
//        public Fix64Vec3 size = Fix64Vec3.one;

//        public Vector3[] verts = new Vector3[0];
//        public int vertsCount = 0;

//        public ParallelQHullData convexData;
//        public ParallelQHullData2 convexData2;

//        ParallelTransform _pTransform;

//        public bool Deterministic = true;

//        public bool ShapeDirty { get; set; }

//        bool _simplified = false;
//        public bool simplified = false;
//        public float angle = 45;

//        UInt32 _limit = 12;
//        public UInt32 Limit = 12;

//        public Fix64Vec3 _currentSize = Fix64Vec3.one;

//        MeshCollider m;

//        public ParallelTransform pTransform
//        {
//            get
//            {
//                if (_pTransform == null)
//                {
//                    _pTransform = GetComponent<ParallelTransform>();
//                }

//                return _pTransform;
//            }
//        }

//        void Update()
//        {
//            //only import from unity in editing mode
//            if (!Deterministic || !Application.isPlaying)
//            {
//                size = (Fix64Vec3)editorSize;

//                if(simplified != _simplified)
//                {
//                    _simplified = simplified;
//                    BuildConvexData();
//                }

//                if(Limit != _limit)
//                {
//                    _limit = Limit;
//                    BuildConvexData();
//                }
//            }
//        }

//        void BuildConvexData()
//        {
//            //stan hull
//            Vector3[] vIn1 = new Vector3[vertsCount];
//            for (int i = 0; i < vertsCount; i++)
//            {
//                vIn1[i] = verts[i];
//            }

//            ParallelQHullData2 qhullData2 = Parallel3D.ConvextHull3D2(vIn1, (UInt32)vertsCount, (int)_limit);

//            convexData2 = qhullData2;

//            ParallelIntTriangle[] t = new ParallelIntTriangle[convexData2.triCount];
//            Array.Copy(convexData2.tris, 0, t, 0, convexData2.triCount);
//            convexData2.tris = t;

//            //new convex hull
//            Fix64Vec3[] vIn = new Fix64Vec3[_limit];
//            for (int i = 0; i < _limit; i++)
//            {
//                vIn[i] = (Fix64Vec3)convexData2.vertices[i];
//            }

//            float rad = angle * Mathf.Deg2Rad;


//            ParallelQHullData qhullData = Parallel3D.ConvextHull3D(vIn, (UInt32)Limit, _simplified, (Fix64)rad);

//            convexData = qhullData;

//            Fix64Vec3[] v = new Fix64Vec3[convexData.vertexCount];
//            Array.Copy(convexData.vertices, 0, v, 0, convexData.vertexCount);
//            convexData.vertices = v;

//            string output = "";
//            output += $"b3Vec3 verts[{convexData.vertexCount}] = {{}};\n";
//            //Debug.Log($"b3Vec3 verts[{convexData.vertexCount}] = {{}};");
//            for (int i = 0; i < convexData.vertexCount; i++)
//            {
//                Vector3 vec3 = (Vector3)convexData.vertices[i];
//                output += $"b3Vec3({vec3.x}, {vec3.y}, {vec3.z}),\n";
//                //Debug.Log($"verts[{i}] = b3Vec3({vec3.x}, {vec3.y}, {vec3.z});");
//            }
//            Debug.Log(output);

//            ParallelEdge[] e = new ParallelEdge[convexData.edgeCount];
//            Array.Copy(convexData.edges, 0, e, 0, convexData.edgeCount);
//            convexData.edges = e;

//            ParallelFace[] f = new ParallelFace[convexData.faceCount];
//            Array.Copy(convexData.faces, 0, f, 0, convexData.faceCount);
//            convexData.faces = f;

//            ParallelPlane[] p = new ParallelPlane[convexData.faceCount];
//            Array.Copy(convexData.planes, 0, p, 0, convexData.faceCount);
//            convexData.planes = p;

//            return;

//            //box3d convex hull
//            /*
//            Fix64Vec3[] vIn = new Fix64Vec3[vertsCount];
//            for (int i = 0; i < vertsCount; i++)
//            {
//                vIn[i] = (Fix64Vec3)verts[i];
//            }

//            float rad = angle * Mathf.Deg2Rad;


//            ParallelQHullData qhullData = Parallel3D.ConvextHull3D(vIn, (UInt32)vertsCount, _simplified, (Fix64)rad);

//            //Fix64Vec3[] vTest = new Fix64Vec3[8];
//            //vTest[0] = new Fix64Vec3(Fix64.FromRaw(-33767), Fix64.FromRaw(865042), Fix64.FromRaw(33746));
//            //vTest[1] = new Fix64Vec3(Fix64.FromRaw(33739), Fix64.FromRaw(865042), Fix64.FromRaw(33747));
//            //vTest[2] = new Fix64Vec3(Fix64.FromRaw(-33766), Fix64.FromRaw(865041), Fix64.FromRaw(-33586));
//            //vTest[3] = new Fix64Vec3(Fix64.FromRaw(-237603), Fix64.FromRaw(0), Fix64.FromRaw(-237597));
//            //vTest[4] = new Fix64Vec3(Fix64.FromRaw(237586), Fix64.FromRaw(-1), Fix64.FromRaw(237602));
//            //vTest[5] = new Fix64Vec3(Fix64.FromRaw(237583), Fix64.FromRaw(-1), Fix64.FromRaw(-237600));
//            //vTest[6] = new Fix64Vec3(Fix64.FromRaw(33739), Fix64.FromRaw(865042), Fix64.FromRaw(-33586));
//            //vTest[7] = new Fix64Vec3(Fix64.FromRaw(-237609), Fix64.FromRaw(-1), Fix64.FromRaw(237599));

//            //ParallelQHullData qhullData = Parallel3D.ConvextHull3D(vTest, (UInt32)8, false);
//            convexData = qhullData;

//            Fix64Vec3[] v = new Fix64Vec3[convexData.vertexCount];
//            Array.Copy(convexData.vertices, 0, v, 0, convexData.vertexCount);
//            convexData.vertices = v;

//            string output = "";
//            output += $"b3Vec3 verts[{convexData.vertexCount}] = {{}};\n";
//            //Debug.Log($"b3Vec3 verts[{convexData.vertexCount}] = {{}};");
//            for (int i = 0; i < convexData.vertexCount; i++)
//            {
//                Vector3 vec3 = (Vector3)convexData.vertices[i];
//                output += $"b3Vec3({vec3.x}, {vec3.y}, {vec3.z}),\n";
//                //Debug.Log($"verts[{i}] = b3Vec3({vec3.x}, {vec3.y}, {vec3.z});");
//            }
//            Debug.Log(output);

//            ParallelEdge[] e = new ParallelEdge[convexData.edgeCount];
//            Array.Copy(convexData.edges, 0, e, 0, convexData.edgeCount);
//            convexData.edges = e;

//            ParallelFace[] f = new ParallelFace[convexData.faceCount];
//            Array.Copy(convexData.faces, 0, f, 0, convexData.faceCount);
//            convexData.faces = f;

//            ParallelPlane[] p = new ParallelPlane[convexData.faceCount];
//            Array.Copy(convexData.planes, 0, p, 0, convexData.faceCount);
//            convexData.planes = p;
//            */
//        }

//        void Reset()
//        {
//            MeshFilter meshFilter = GetComponent<MeshFilter>();

//            Mesh mesh = meshFilter.sharedMesh;

//            verts = mesh.vertices;
//            vertsCount = verts.Length;

//            BuildConvexData();
//        }

//        void DrawMesh()
//        {
//            Gizmos.color = Color.yellow;
//            m = GetComponent<MeshCollider>();
//            Mesh mesh1 = m.sharedMesh;
//            for (int i = 0; i < mesh1.vertexCount; i++)
//            {
//                Vector3 v = mesh1.vertices[i];
//                Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
//            }
//        }

//        void DrawStanHull()
//        {
//            for (int i = 0; i < convexData2.triCount; i++)
//            {
//                ParallelIntTriangle t = convexData2.tris[i];
//                Vector3 v1 = convexData2.vertices[t.v1];
//                Vector3 v2 = convexData2.vertices[t.v2];
//                Vector3 v3 = convexData2.vertices[t.v3];

//                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v2.x, v2.y, v2.z));
//                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v3.x, v3.y, v3.z));
//                Gizmos.DrawLine(transform.TransformPoint(v3.x, v3.y, v3.z), transform.TransformPoint(v2.x, v2.y, v2.z));
//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            //Gizmos.color = Color.cyan;
//            //DrawStanHull();
//            //return;
//            //DrawMesh();



//            //foreach (Vector3 v in verts)
//            //{
//            //    Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
//            //}

//            Gizmos.color = Color.red;
//            //for(UInt32 i = 0; i < convexData.faceCount; i++)
//            //{
//            //    ParallelEdge edge1 = convexData.edges[convexData.faces[i].edge];
//            //    ParallelEdge edge2 = convexData.edges[convexData.faces[i].edge + 1];

//            //    Vector3 v1 = (Vector3)convexData.vertices[edge1.origin];
//            //    Vector3 v2 = (Vector3)convexData.vertices[edge2.origin];

//            //    Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v2.x, v2.y, v2.z));
//            //}
//            for (UInt32 i = 0; i < convexData.edgeCount; i += 2)
//            {
//                ParallelEdge edge1 = convexData.edges[i];
//                ParallelEdge edge2 = convexData.edges[i + 1];

//                Vector3 v1 = (Vector3)convexData.vertices[edge1.origin];
//                Vector3 v2 = (Vector3)convexData.vertices[edge2.origin];

//                Gizmos.DrawLine(transform.TransformPoint(v1.x, v1.y, v1.z), transform.TransformPoint(v2.x, v2.y, v2.z));
//            }


//            Gizmos.color = Color.yellow;
//            foreach (Fix64Vec3 fv in convexData.vertices)
//            {
//                Vector3 v = (Vector3)fv;
//                Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
//            }
//        }

//        Fix64Vec3 CalculateSize()
//        {
//            Fix64Vec3 s = size * pTransform.localScale;

//            if (s.x > Fix64.zero && s.y > Fix64.zero)
//            {
//                return s;
//            }
//            else
//            {
//                Debug.LogError("Invalid size");
//                return Fix64Vec3.zero;
//            }
//        }

//        public PShape3D CreateShape()
//        {
//            Fix64Vec3 s = CalculateSize();

//            if (s != Fix64Vec3.zero)
//            {
//                _currentSize = s;
//                _shape = Parallel3D.CreatePolyhedron(convexData, s);
//                return _shape;
//            }
//            else
//            {
//                return null;
//            }
//        }

//        public void ReceiveFixture(PFixture3D fixture)
//        {
//            _fixture = fixture;
//        }

//        public void UpdateNativeShapeIfNecessary()
//        {
//            if (!ShapeDirty)
//            {
//                return;
//            }

//            ShapeDirty = false;

//            Fix64Vec3 s = CalculateSize();
//            if (s == Fix64Vec3.zero)
//            {
//                return;
//            }

//            if (s == _currentSize)
//            {
//                return;
//            }

//            Fix64Vec3 scale = s / _currentSize;

//            _currentSize = s;

//            Parallel3D.UpdatePolyhedron(_shape, _fixture, scale);
//        }
//    }
//}
