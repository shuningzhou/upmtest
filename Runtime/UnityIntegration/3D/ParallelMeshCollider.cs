using UnityEngine;
using ParallelUnity.DebugTools;
using System;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public class ParallelMeshCollider : ParallelCollider3D
    {
        MeshFilter meshFilter;
        Mesh mesh;

        public float gizmoSize = 0.005f;
        public Vector3[] verts = new Vector3[0];
        public int vertsCount = 0;
        public ParallelMeshData meshData;

        Fix64Vec3 _currentSize = Fix64Vec3.one;

        void Reset()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            Mesh mesh = meshFilter.sharedMesh;

            verts = mesh.vertices;
            vertsCount = verts.Length;

            int[] triangles = mesh.triangles;
            int triangleCount = triangles.Length;

            meshData = new ParallelMeshData();
            meshData.vertexCount = (UInt32)vertsCount;
            meshData.vertices = new Fix64Vec3[meshData.vertexCount];
            meshData.triangleCount = (UInt32)(triangleCount / 3);
            meshData.triangles = new ParallelTriangle[meshData.triangleCount];

            for (int i = 0; i < vertsCount; i++)
            {
                meshData.vertices[i] = (Fix64Vec3)verts[i];
            }

            for (int i = 0; i < triangleCount; i++)
            {
                int vIndex = i % 3;
                int tIndex = i / 3;

                if (vIndex == 0)
                {
                    meshData.triangles[tIndex].v1 = (UInt32)triangles[i];
                }
                else if (vIndex == 1)
                {
                    meshData.triangles[tIndex].v2 = (UInt32)triangles[i];
                }
                else
                {
                    meshData.triangles[tIndex].v3 = (UInt32)triangles[i];
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            foreach (Vector3 v in verts)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
            }

            Gizmos.color = Color.red;
            foreach (Fix64Vec3 fv in meshData.vertices)
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

            Parallel3D.UpdateMesh(_shape, _fixture, scale);
        }

        public override PShape3D CreateShape(GameObject root)
        {
            Fix64Vec3 s = CalculateSize();

            if (s != Fix64Vec3.zero)
            {
                _currentSize = s;
                _shape = Parallel3D.CreateMesh(meshData, s);

                if(createUnityPhysicsCollider)
                {
                    gameObject.AddComponent<MeshCollider>();
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
//    public class PMeshCollider : MonoBehaviour, IParallelCollider3D
//    {
//        PShape3D _shape;
//        PFixture3D _fixture;

//        MeshFilter meshFilter;
//        Mesh mesh;

//        public Vector3 editorSize = Vector3.one;
//        public Fix64Vec3 size = Fix64Vec3.one;

//        public float gizmoSize = 0.005f;
//        public Vector3[] verts = new Vector3[0];
//        public int vertsCount = 0;
//        public ParallelMeshData meshData;

//        ParallelTransform _pTransform;

//        public Fix64Vec3 _currentSize = Fix64Vec3.one;

//        public bool Deterministic = true;

//        public bool ShapeDirty { get; set; }

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
//            }
//        }

//        void Reset()
//        {
//            MeshFilter meshFilter = GetComponent<MeshFilter>();

//            Mesh mesh = meshFilter.sharedMesh;

//            verts = mesh.vertices;
//            vertsCount = verts.Length;

//            int[] triangles = mesh.triangles;
//            int triangleCount = triangles.Length;

//            meshData = new ParallelMeshData();
//            meshData.vertexCount = (UInt32)vertsCount;
//            meshData.vertices = new Fix64Vec3[meshData.vertexCount];
//            meshData.triangleCount = (UInt32)(triangleCount / 3);
//            meshData.triangles = new ParallelTriangle[meshData.triangleCount];

//            for (int i = 0; i < vertsCount; i++)
//            {
//                meshData.vertices[i] = (Fix64Vec3)verts[i];
//            }

//            for (int i = 0; i < triangleCount; i++)
//            {
//                int vIndex = i % 3;
//                int tIndex = i / 3;

//                if (vIndex == 0)
//                {
//                    meshData.triangles[tIndex].v1 = (UInt32)triangles[i];
//                }
//                else if (vIndex == 1)
//                {
//                    meshData.triangles[tIndex].v2 = (UInt32)triangles[i];
//                }
//                else
//                {
//                    meshData.triangles[tIndex].v3 = (UInt32)triangles[i];
//                }
//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            Gizmos.color = Color.cyan;

//            foreach (Vector3 v in verts)
//            {
//                Gizmos.DrawWireSphere(transform.TransformPoint(v.x, v.y, v.z), 0.01f);
//            }

//            Gizmos.color = Color.red;
//            foreach (Fix64Vec3 fv in meshData.vertices)
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
//                _shape = Parallel3D.CreateMesh(meshData, s);
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

//            Parallel3D.UpdateMesh(_shape, _fixture, scale);
//        }
//    }
//}
