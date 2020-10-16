using UnityEngine;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    public abstract class ParallelCollider3D : MonoBehaviour
    {
        protected PShape3D _shape;
        protected PFixture3D _fixture;
        protected ParallelTransform _pTransform;

        public bool isTrigger = false;
        public bool createUnityPhysicsCollider = false;
        
        [SerializeField]
        Fix64 _friction = Fix64.FromDivision(4, 10);

        [SerializeField]
        Fix64 _bounciness = Fix64.FromDivision(2, 10);

        public bool ShapeDirty { get; set; }

        public Fix64 friction
        {
            get
            {
                return _friction;
            }
            set
            {
                _friction = value;
            }
        }

        public Fix64 bounciness
        {
            get
            {
                return _bounciness;
            }
            set
            {
                _bounciness = value;
            }
        }

        public ParallelTransform pTransform
        {
            get
            {
                if (_pTransform == null)
                {
                    _pTransform = GetComponent<ParallelTransform>();
                }

                return _pTransform;
            }
        }

        protected abstract void UpdateShape(GameObject root);
        public abstract PShape3D CreateShape(GameObject root);

        public void ReceiveFixture(PFixture3D fixture)
        {
            _fixture = fixture;
            _shape = Parallel3D.GetShapeOfFixture(fixture);
            Parallel3D.SetLayer(fixture, gameObject.layer, false);
            Parallel3D.SetFixtureProperties(fixture, isTrigger, _friction, _bounciness);
        }

        public void UpdateNativeShapeIfNecessary(GameObject root)
        {
            if (!ShapeDirty)
            {
                return;
            }

            UpdateShape(root);

            ShapeDirty = false;
        }
    }
}
