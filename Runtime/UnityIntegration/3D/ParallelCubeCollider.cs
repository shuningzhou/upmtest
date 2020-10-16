using UnityEngine;
using ParallelUnity.DebugTools;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    public class ParallelCubeCollider : ParallelCollider3D
    {
        [SerializeField]
        Fix64Vec3 _size = Fix64Vec3.one;

        public Fix64Vec3 size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        void OnDrawGizmosSelected()
        {
            Fix64Vec3 s = CalculateSize();
            Gizmos.color = DebugSettings.ColliderOutlineColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, (Vector3)s);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Fix64Vec3 CalculateSize()
        {
            Fix64Vec3 s = size * pTransform.localScale;

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
            if (s != Fix64Vec3.zero)
            {
                Fix64Vec3 center = Fix64Vec3.zero;
                Fix64Quat rotation = Fix64Quat.identity;

                if(gameObject != root)
                {
                    center = _pTransform.localPosition;
                    rotation = _pTransform.localRotation;
                }

                Parallel3D.UpdateCube(_shape, _fixture, s.x, s.y, s.z, center, rotation);
            }
        }

        public override PShape3D CreateShape(GameObject root)
        {
            Fix64Vec3 s = CalculateSize();

            if (s != Fix64Vec3.zero)
            {
                Fix64Vec3 center = Fix64Vec3.zero;
                Fix64Quat rotation = Fix64Quat.identity;

                if (gameObject != root)
                {
                    center = _pTransform.localPosition;
                    rotation = _pTransform.localRotation;
                }

                _shape = Parallel3D.CreateCube(s.x, s.y, s.z, center, rotation);

                if(createUnityPhysicsCollider)
                {
                    var collider = gameObject.AddComponent<BoxCollider>();
                    collider.size = (Vector3)size;
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
//    public class PCubeCollider : MonoBehaviour, IParallelCollider3D
//    {
//        PShape3D _shape;
//        PFixture3D _fixture;

//        public Vector3 editorSize = Vector3.one;
//        public Fix64Vec3 size = Fix64Vec3.one;

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
//                size = (Fix64Vec3)editorSize;
//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            Fix64Vec3 s = CalculateSize();
//            Gizmos.color = DebugSettings.ColliderOutlineColor;
//            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
//            Gizmos.DrawWireCube(Vector3.zero, (Vector3)s);
//            Gizmos.matrix = Matrix4x4.identity;
//        }

//        Fix64Vec3 CalculateSize()
//        {
//            Fix64Vec3 s = size * pTransform.localScale;//Vector2.Scale(size, transform.localScale);

//            //Debug.Log($"realSize={realSize}, localScale={pTransform.localScale}, s={s}");

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
//                _shape = Parallel3D.CreateCube(s.x, s.y, s.z);
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

//            Fix64Vec3 s = CalculateSize();
//            if (s != Fix64Vec3.zero)
//            {
//                Parallel3D.UpdateCube(_shape, _fixture, s.x, s.y, s.z);
//                ShapeDirty = false;
//            }
//        }
//    }
//}
