using UnityEngine;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    [ExecuteInEditMode]
    public abstract class ParallelCollider2D : MonoBehaviour
    {
        protected PShape2D _shape;
        protected PFixture2D _fixture;
        protected ParallelTransform _pTransform;

        protected GameObject _root;

         [SerializeField]
        bool isTrigger = false;
        
        [SerializeField]
        Fix64 _friction = Fix64.FromDivision(4, 10);

        [SerializeField]
        Fix64 _bounciness = Fix64.FromDivision(2, 10);

        [SerializeField]
        Fix64Vec2 _spriteRendererSize = Fix64Vec2.one;

        protected Fix64Vec3 colliderScale
        {
            get
            {
                Fix64 x = pTransform.localScale.x * _spriteRendererSize.x;
                Fix64 y = pTransform.localScale.y * _spriteRendererSize.y;
                Fix64 z = pTransform.localScale.z;

                return new Fix64Vec3(x, y, z);
            }
        }

        SpriteRenderer spriteRenderer
        {
            get
            {
                return GetComponent<SpriteRenderer>();
            }
        }

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
        public abstract PShape2D CreateShape(GameObject root);

        public void SetRootGameObject(GameObject root)
        {
            _root = root;
        }
        
        public void ReceiveFixture(PFixture2D fixture)
        {
            _fixture = fixture;
            _shape = Parallel2D.GetShapeOfFixture(fixture);
            Parallel2D.SetLayer(fixture, gameObject.layer, false);
            Parallel2D.SetFixtureProperties(fixture, isTrigger, _friction, _bounciness);
        }

        public void UpdateNativeShapeIfNecessary(GameObject root)
        {
            UpdateShape(root);
        }

        //============================== Unity Events ==============================
        void Update()
        {
            //only import from unity if in editing mode
            if (!Application.isPlaying)
            {
                if(spriteRenderer == null)
                {
                    _spriteRendererSize = Fix64Vec2.one;
                }
                else
                {
                    _spriteRendererSize = (Fix64Vec2)spriteRenderer.size;
                }
            }
        }
    }
}
