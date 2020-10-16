using UnityEngine;
using ParallelUnity.DebugTools;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public class ParallelCircleCollider : ParallelCollider2D
    {
        [SerializeField]
        Fix64 _radius = Fix64.FromDivision(1,2);

        public Fix64 radius
        {
            get
            {
                return _radius;
            }
        }

        public void UpdateShape(Fix64 radius)
        {
            _radius = radius;
            UpdateShape(_root);
        }

        void OnDrawGizmosSelected()
        {
            Fix64 r = CalculateRadius();
            if(r <= Fix64.zero)
            {
                Debug.LogError("Invalid Size");
                return;
            }
            Gizmos.color = DebugSettings.ColliderOutlineColor;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, (float)r);
            Gizmos.matrix = Matrix4x4.identity;
        }

        Fix64 CalculateRadius()
        {
            Fix64 maxScale = Fix64Math.Max(colliderScale.x, colliderScale.y);
            Fix64 result = maxScale * _radius;
            return result;
        }

        protected override void UpdateShape(GameObject root)
        {
            Fix64 r = CalculateRadius();
            if (r <= Fix64.zero)
            {
                Debug.LogError("Invalid Size");
            }
            else
            {
                Fix64Vec2 center = Fix64Vec2.zero;

                if (gameObject != root)
                {
                    center = (Fix64Vec2)_pTransform.localPosition;
                }

                Parallel2D.UpdateCircle(_shape, _fixture, r, center);
            }
        }

        public override PShape2D CreateShape(GameObject root)
        {
            Fix64 r = CalculateRadius();
            if (r <= Fix64.zero)
            {
                Debug.LogError("Invalid Size");
                return null;
            }
            else
            {
                Fix64Vec2 center = Fix64Vec2.zero;

                if (gameObject != root)
                {
                    center = (Fix64Vec2)_pTransform.localPosition;
                }

                return Parallel2D.CreateCircle(r, center);
            }
        }
    }
}

//namespace Parallel
//{
//    [RequireComponent(typeof(ParallelTransform))]
//    [ExecuteInEditMode]
//    public class PCircleCollider : MonoBehaviour, IParallelCollider2D
//    {
//        PShape2D _shape;
//        PFixture2D _fixture;

//        ParallelTransform _pTransform;

//        public Fix64 Radius = Fix64.FromDivision(1, 2);

//        public bool Deterministic = true;

//        public bool isTrigger = false;
//        public Fix64 friction = Fix64.FromDivision(4, 10);
//        public Fix64 bounciness = Fix64.FromDivision(2, 10);

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

//            }
//        }

//        void OnDrawGizmosSelected()
//        {
//            Fix64 r = CalculateRadius();
//            if (r <= Fix64.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return;
//            }
//            Gizmos.color = DebugSettings.ColliderOutlineColor;
//            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
//            Gizmos.DrawWireSphere(Vector3.zero, (float)r);
//            Gizmos.matrix = Matrix4x4.identity;
//        }

//        Fix64 CalculateRadius()
//        {
//            Fix64 maxScale = Fix64Math.Max(pTransform.localScale.x, pTransform.localScale.y);
//            Fix64 result = maxScale * Radius;
//            return result;
//        }

//        public PShape2D CreateShape(GameObject root)
//        {
//            if (gameObject == root)
//            {

//            }
//            Fix64 r = CalculateRadius();
//            if (r <= Fix64.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return null;
//            }
//            else
//            {
//                Fix64Vec2 center = Fix64Vec2.zero;

//                if (gameObject != root)
//                {
//                    center = (Fix64Vec2)_pTransform.localPosition;
//                }

//                return Parallel2D.CreateCircle(r, center);
//            }
//        }

//        public void ReceiveFixture(PFixture2D fixture)
//        {
//            _fixture = fixture;
//            _shape = Parallel2D.GetShapeOfFixture(fixture);
//            Parallel2D.SetLayer(fixture, gameObject.layer, false);
//            Parallel2D.SetFixtureProperties(fixture, isTrigger, friction, bounciness);
//        }

//        public void UpdateNativeShapeIfNecessary(GameObject root)
//        {
//            if (!ShapeDirty)
//            {
//                return;
//            }

//            Fix64 r = CalculateRadius();
//            if (r <= Fix64.zero)
//            {
//                Debug.LogError("Invalid Size");
//                return;
//            }
//            else
//            {
//                Fix64Vec2 center = Fix64Vec2.zero;

//                if (gameObject != root)
//                {
//                    center = (Fix64Vec2)_pTransform.localPosition;
//                }

//                Parallel2D.UpdateCircle(_shape, _fixture, r, center);
//                ShapeDirty = false;
//            }
//        }
//    }
//}