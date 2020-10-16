using UnityEngine;
using ParallelUnity.DebugTools;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public class ParallelCapsuleCollider3D : ParallelCollider3D
    {
        public enum ParallelCapsuleDirection3D
        {
            XAxis = 0,
            YAxis = 1,
            ZAxis = 2
        }

        public ParallelCapsuleDirection3D Direction = ParallelCapsuleDirection3D.YAxis;

        [SerializeField]
        Fix64 _radius = Fix64.FromDivision(1, 2);
        [SerializeField]
        Fix64 _height = Fix64.FromDivision(2, 1);

        public Fix64 height { get; private set; }
        public Fix64 radius { get; private set; }

        public Fix64Vec3 point1;
        public Fix64Vec3 point2;

        void OnDrawGizmosSelected()
        {
            Fix64 h = CalculateHeight();
            Fix64 r = CalculateRadius();
            Fix64Vec3 p1 = Fix64Vec3.zero;
            Fix64Vec3 p2 = Fix64Vec3.zero;
            CalculatePoints(h, r, ref p1, ref p2);

            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
            {
                Debug.LogError("Invalid Size");
                return;
            }

            Gizmos.color = DebugSettings.ColliderOutlineColor;
            Vector3 point1 = PMath.TransformPointUnscaled(transform, (Vector3)p1);
            Vector3 point2 = PMath.TransformPointUnscaled(transform, (Vector3)p2);

            Vector3 origin = (point1 - point2) / 2 + point1;
            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3((float)r, (float)r, (float)r));
            DebugDraw.DrawHemispheresOfCapsule(point1, point2, (float)r);
            Gizmos.matrix = Matrix4x4.identity;
            DebugDraw.DrawLineConnectingHS(point1, point2, (float)r);
            Gizmos.matrix = Matrix4x4.identity;
        }

        void CalculatePoints(Fix64 h, Fix64 r, ref Fix64Vec3 point1, ref Fix64Vec3 point2)
        {
            point1 = Fix64Vec3.zero;
            point2 = Fix64Vec3.zero;

            Fix64 pointDistance = h - Fix64.FromDivision(2 , 1) * r;

            if (pointDistance <= Fix64.zero)
            {
                Debug.LogError("Invalid size");
                return;
            }

            if (Direction == ParallelCapsuleDirection3D.XAxis)
            {
                point1 = new Fix64Vec3(Fix64.one, Fix64.zero, Fix64.zero);
                point2 = new Fix64Vec3(-Fix64.one, Fix64.zero, Fix64.zero);
            }
            else if (Direction == ParallelCapsuleDirection3D.YAxis)
            {
                point1 = new Fix64Vec3(Fix64.zero, Fix64.one, Fix64.zero);
                point2 = new Fix64Vec3(Fix64.zero, -Fix64.one, Fix64.zero);
            }
            else
            {
                point1 = new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.one);
                point2 = new Fix64Vec3(Fix64.zero, Fix64.zero, -Fix64.one);
            }

            point1 = point1 * (pointDistance / Fix64.FromDivision(2, 1));
            point2 = point2 * (pointDistance / Fix64.FromDivision(2, 1));
        }

        Fix64 CalculateRadius()
        {
            Fix64 maxScale = Fix64.one;

            if (Direction == ParallelCapsuleDirection3D.XAxis)
            {
                maxScale = Fix64Math.Max(pTransform.localScale.y, pTransform.localScale.z);
            }
            else if (Direction == ParallelCapsuleDirection3D.YAxis)
            {
                maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.z);
            }
            else
            {
                maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.y);
            }

            return maxScale * _radius;
        }

        Fix64 CalculateHeight()
        {
            Fix64 maxScale = Fix64.one;

            if (Direction == ParallelCapsuleDirection3D.XAxis)
            {
                maxScale = pTransform.localScale.x;
            }
            else if (Direction == ParallelCapsuleDirection3D.YAxis)
            {
                maxScale = pTransform.localScale.y;
            }
            else
            {
                maxScale = pTransform.localScale.z;
            }

            return maxScale * _height;
        }

        protected override void UpdateShape(GameObject root)
        {
            Fix64 h = CalculateHeight();
            Fix64 r = CalculateRadius();
            Fix64Vec3 p1 = Fix64Vec3.zero;
            Fix64Vec3 p2 = Fix64Vec3.zero;
            CalculatePoints(h, r, ref p1, ref p2);

            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
            {
                Debug.LogError("Invalid Size");
                return;
            }

            Fix64Vec3 center = Fix64Vec3.zero;
            Fix64Quat rotation = Fix64Quat.identity;

            if (gameObject != root)
            {
                center = _pTransform.localPosition;
                rotation = _pTransform.localRotation;
            }

            point1 = p1;
            point2 = p2;
            radius = r;
            height = h;
            Parallel3D.UpdateCapsule(_shape, _fixture, p1, p2, radius, center, rotation);
        }

        public override PShape3D CreateShape(GameObject root)
        {
            Fix64 h = CalculateHeight();
            Fix64 r = CalculateRadius();
            Fix64Vec3 p1 = Fix64Vec3.zero;
            Fix64Vec3 p2 = Fix64Vec3.zero;
            CalculatePoints(h, r, ref p1, ref p2);

            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
            {
                Debug.LogError("Invalid Size");
                return null;
            }
            else
            {
                Fix64Vec3 center = Fix64Vec3.zero;
                Fix64Quat rotation = Fix64Quat.identity;

                if (gameObject != root)
                {
                    center = _pTransform.localPosition;
                    rotation = _pTransform.localRotation;
                }

                point1 = p1;
                point2 = p2;
                radius = r;
                height = h;
                _shape = Parallel3D.CreateCapsule(p1, p2, radius, center, rotation);

                if(createUnityPhysicsCollider)
                {
                    var collider = gameObject.AddComponent<CapsuleCollider>();
                    collider.height = (float)h;
                    collider.radius = (float)radius;
                }

                return _shape;
            }
        }
    }
}

//namespace Parallel
//{
//    [RequireComponent(typeof(ParallelTransform))]
//    [ExecuteInEditMode]
//    public class PCapsuleCollider3D : MonoBehaviour, IParallelCollider3D
//    {
//        public enum PCapsuleDirection3D
//        {
//            XAxis = 0,
//            YAxis = 1,
//            ZAxis = 2
//        }

//        PShape3D _shape;
//        PFixture3D _fixture;

//        public Fix64 Radius = Fix64.FromDivision(1, 2);
//        public Fix64 Height = Fix64.FromDivision(2, 1);

//        public PCapsuleDirection3D Direction = PCapsuleDirection3D.YAxis;

//        public Fix64Vec3 Point1;
//        public Fix64Vec3 Point2;

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
//            Fix64 height = CalculateHeight();
//            Fix64 radius = CalculateRadius();
//            Fix64Vec3 p1 = Fix64Vec3.zero;
//            Fix64Vec3 p2 = Fix64Vec3.zero;
//            CalculatePoints(height, radius, ref p1, ref p2);

//            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return;
//            }

//            Gizmos.color = DebugSettings.ColliderOutlineColor;
//            Vector3 point1 = PMath.TransformPointUnscaled(transform, (Vector3)p1);
//            Vector3 point2 = PMath.TransformPointUnscaled(transform, (Vector3)p2);

//            Vector3 origin = (point1 - point2) / 2 + point1;
//            Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3((float)radius, (float)radius, (float)radius));
//            DebugDraw.DrawHemispheresOfCapsule(point1, point2, (float)radius);
//            Gizmos.matrix = Matrix4x4.identity;
//            DebugDraw.DrawLineConnectingHS(point1, point2, (float)radius);
//            Gizmos.matrix = Matrix4x4.identity;
//        }

//        void CalculatePoints(Fix64 height, Fix64 radius, ref Fix64Vec3 point1, ref Fix64Vec3 point2)
//        {
//            point1 = Fix64Vec3.zero;
//            point2 = Fix64Vec3.zero;

//            Fix64 pointDistance = height - Fix64.FromDivision(2, 1) * radius;

//            if (pointDistance <= Fix64.zero)
//            {
//                Debug.LogError("Invalid size");
//                return;
//            }

//            if (Direction == PCapsuleDirection3D.XAxis)
//            {
//                point1 = new Fix64Vec3(Fix64.one, Fix64.zero, Fix64.zero);
//                point2 = new Fix64Vec3(-Fix64.one, Fix64.zero, Fix64.zero);
//            }
//            else if (Direction == PCapsuleDirection3D.YAxis)
//            {
//                point1 = new Fix64Vec3(Fix64.zero, Fix64.one, Fix64.zero);
//                point2 = new Fix64Vec3(Fix64.zero, -Fix64.one, Fix64.zero);
//            }
//            else
//            {
//                point1 = new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.one);
//                point2 = new Fix64Vec3(Fix64.zero, Fix64.zero, -Fix64.one);
//            }

//            point1 = point1 * (pointDistance / Fix64.FromDivision(2, 1));
//            point2 = point2 * (pointDistance / Fix64.FromDivision(2, 1));
//        }

//        Fix64 CalculateRadius()
//        {
//            Fix64 maxScale = Fix64.one;

//            if (Direction == PCapsuleDirection3D.XAxis)
//            {
//                maxScale = Fix64Math.Max(pTransform.localScale.y, pTransform.localScale.z);
//            }
//            else if (Direction == PCapsuleDirection3D.YAxis)
//            {
//                maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.z);
//            }
//            else
//            {
//                maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.y);
//            }

//            return maxScale * Radius;
//        }

//        Fix64 CalculateHeight()
//        {
//            Fix64 maxScale = Fix64.one;

//            if (Direction == PCapsuleDirection3D.XAxis)
//            {
//                maxScale = pTransform.localScale.x;
//            }
//            else if (Direction == PCapsuleDirection3D.YAxis)
//            {
//                maxScale = pTransform.localScale.y;
//            }
//            else
//            {
//                maxScale = pTransform.localScale.z;
//            }

//            return maxScale * Height;
//        }

//        public PShape3D CreateShape()
//        {
//            Fix64 height = CalculateHeight();
//            Fix64 radius = CalculateRadius();
//            Fix64Vec3 p1 = Fix64Vec3.zero;
//            Fix64Vec3 p2 = Fix64Vec3.zero;
//            CalculatePoints(height, radius, ref p1, ref p2);

//            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return null;
//            }

//            _shape = Parallel3D.CreateCapsule(p1, p2, radius);
//            return _shape;
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

//            Fix64 height = CalculateHeight();
//            Fix64 radius = CalculateRadius();
//            Fix64Vec3 p1 = Fix64Vec3.zero;
//            Fix64Vec3 p2 = Fix64Vec3.zero;
//            CalculatePoints(height, radius, ref p1, ref p2);

//            if (p1 == Fix64Vec3.zero || p2 == Fix64Vec3.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return;
//            }

//            Parallel3D.UpdateCapsule(_shape, _fixture, p1, p2, radius);
//            ShapeDirty = false;
//        }
//    }
//}
