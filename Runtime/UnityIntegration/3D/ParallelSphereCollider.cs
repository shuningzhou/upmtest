using UnityEngine;
using ParallelUnity.DebugTools;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public class ParallelSphereCollider : ParallelCollider3D
    {
        [SerializeField]
        Fix64 _radius = Fix64.FromDivision(1, 2);

        public Fix64 radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
            }
        }

        void OnDrawGizmosSelected()
        {
            Fix64 r = CalculateRadius();

            if (r > Fix64.zero)
            {
                Gizmos.color = DebugSettings.ColliderOutlineColor;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 1, 1));
                DebugDraw.DrawHemiSphere(Vector3.zero, (float)r, Vector3.up);
                DebugDraw.DrawHemiSphere(Vector3.zero, (float)r, Vector3.down);
                Gizmos.matrix = Matrix4x4.identity;
            }
            else
            {
                Debug.LogError("Invalid Size");
            }
        }

        Fix64 CalculateRadius()
        {
            Fix64 maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.y, pTransform.localScale.z);
            Fix64 result = maxScale * _radius;
            return result;
        }

        protected override void UpdateShape(GameObject root)
        {
            Fix64 r = CalculateRadius();

            if (r > Fix64.zero)
            {
                Fix64Vec3 center = Fix64Vec3.zero;

                if (gameObject != root)
                {
                    center = _pTransform.localPosition;
                }

                Parallel3D.UpdateSphere(_shape, _fixture, r, center);
            }
        }

        public override PShape3D CreateShape(GameObject root)
        {
            Fix64 r = CalculateRadius();

            if (r > Fix64.zero)
            {
                Fix64Vec3 center = Fix64Vec3.zero;

                if (gameObject != root)
                {
                    center = _pTransform.localPosition;
                }

                _shape = Parallel3D.CreateSphere(r, center);

                if(createUnityPhysicsCollider)
                {
                    var collider = gameObject.AddComponent<SphereCollider>();
                    collider.radius = (float)radius;
                }

                return _shape;
            }
            else
            {
                Debug.LogError("Invalid Size");
                return null;
            }
        }
    }
}

//namespace Parallel
//{
//    [RequireComponent(typeof(ParallelTransform))]
//    [ExecuteInEditMode]
//    public class PSphereCollider : MonoBehaviour, IParallelCollider3D
//    {
//        PShape3D _shape;
//        PFixture3D _fixture;

//        public Fix64 Radius = Fix64.FromDivision(1, 2);

//        ParallelTransform _pTransform;

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
//                //size = (Fix64Vec3)editorSize;
//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            Fix64 r = CalculateRadius();

//            if (r > Fix64.zero)
//            {
//                Gizmos.color = DebugSettings.ColliderOutlineColor;
//                Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 1, 1));
//                DebugDraw.DrawHemiSphere(Vector3.zero, (float)r, Vector3.up);
//                DebugDraw.DrawHemiSphere(Vector3.zero, (float)r, Vector3.down);
//                Gizmos.matrix = Matrix4x4.identity;
//            }
//            else
//            {
//                Debug.LogError("Invalid Size");
//            }
//        }

//        Fix64 CalculateRadius()
//        {
//            Fix64 maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.y, pTransform.localScale.z);
//            Fix64 result = maxScale * Radius;
//            return result;
//        }

//        public PShape3D CreateShape()
//        {
//            Fix64 r = CalculateRadius();

//            if (r > Fix64.zero)
//            {
//                _shape = Parallel3D.CreateSphere(r);
//                return _shape;
//            }
//            else
//            {
//                Debug.LogError("Invalid Size");
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

//            Fix64 r = CalculateRadius();

//            if (r > Fix64.zero)
//            {
//                Parallel3D.UpdateSphere(_shape, _fixture, r);
//                ShapeDirty = false;
//            }
//        }
//    }
//}
