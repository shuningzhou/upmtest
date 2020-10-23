using System;
using UnityEngine;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    public class ParallelRigidbody2D : MonoBehaviour, IParallelRigidbody2D, IReplayable
    {
        ParallelTransform _pTransform;
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

        IParallelFixedUpdate[] parallelFixedUpdates = new IParallelFixedUpdate[0];
        IParallelCollision2D[] parallelCollisions = new IParallelCollision2D[0];
        IParallelTrigger2D[] parallelTriggers = new IParallelTrigger2D[0];
        ParallelCollider2D[] colliders = new ParallelCollider2D[0];

        public PBody2D _body2D;
        [SerializeField]
        int _bodyID;

        //============================== Body properties ==============================
        [SerializeField]
        BodyType _bodyType = Parallel.BodyType.Dynamic;
        [SerializeField]
        Fix64 _linearDampling = Fix64.zero;
        [SerializeField]
        Fix64 _angularDamping = Fix64.FromDivision(5, 100);
        [SerializeField]
        Fix64 _gravityScale = Fix64.one;
        [SerializeField]
        bool _fixedRotation = false;

        public BodyType bodyType
        {
            get
            {
                return _bodyType;
            }
            set
            {
                _bodyType = value;
                Parallel2D.UpdateBodyProperties(_body2D, (int)bodyType, linearDampling, angularDamping, fixedRotation, gravityScale);
            }
        }

        public Fix64 linearDampling
        {
            get
            {
                return _linearDampling;
            }
            set
            {
                _linearDampling = value;
                Parallel2D.UpdateBodyProperties(_body2D, (int)bodyType, linearDampling, angularDamping, fixedRotation, gravityScale);
            }
        }

        public Fix64 angularDamping
        {
            get
            {
                return _angularDamping;
            }
            set
            {
                _angularDamping = value;
                Parallel2D.UpdateBodyProperties(_body2D, (int)bodyType, linearDampling, angularDamping, fixedRotation, gravityScale);
            }
        }

        public Fix64 gravityScale
        {
            get
            {
                return _gravityScale;
            }
            set
            {
                _gravityScale = value;
                Parallel2D.UpdateBodyProperties(_body2D, (int)bodyType, linearDampling, angularDamping, fixedRotation, gravityScale);
            }
        }

        public bool fixedRotation
        {
            get
            {
                return _fixedRotation;
            }
            set
            {
                _fixedRotation = value;
                Parallel2D.UpdateBodyProperties(_body2D, (int)bodyType, linearDampling, angularDamping, fixedRotation, gravityScale);
            }
        }

        //============================== Velocity ==============================
        public Fix64Vec2 LinearVelocity
        {
            get
            {
                return _body2D.linearVelocity;
            }
            set
            {
                _body2D.linearVelocity = value;
                 Parallel2D.UpdateBodyVelocity(_body2D, LinearVelocity, AngularVelocity);
            }
        }

        // rad/sec, z-axis (out of the screen)
        public Fix64 AngularVelocity
        {
            get
            {
                return _body2D.angularVelocity;
            }
            set
            {
                _body2D.angularVelocity = value;
                 Parallel2D.UpdateBodyVelocity(_body2D, LinearVelocity, AngularVelocity);
            }
        }

        //============================== Force and Torque ==============================
        //Apply a force to the center of mass
        public void ApplyForce(Fix64Vec2 force)
        {
            Parallel2D.ApplyForceToCenter(_body2D, force);
        }

        //Apply a force at a world point
        public void ApplyForce(Fix64Vec2 force, Fix64Vec2 worldPoint)
        {
            Parallel2D.ApplyForce(_body2D, worldPoint, force);
        }

        //Apply an impulse to the center of mass. This immediately modifies the velocity.
        public void ApplyLinearImpulse(Fix64Vec2 impluse)
        {
            Parallel2D.ApplyLinearImpulseToCenter(_body2D, impluse);
        }

        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        public void ApplyLinearImpluse(Fix64Vec2 impluse, Fix64Vec2 worldPoint)
        {
            Parallel2D.ApplyLinearImpulse(_body2D, worldPoint, impluse);
        }

        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// z-axis (out of the screen)
        public void ApplyTorque(Fix64 torque)
        {
            Parallel2D.ApplyTorque(_body2D, torque);
        }

        /// Apply an angular impulse. This immediately modifies the angular velocity
        public void ApplyAngularImpulse(Fix64 impulse)
        {
            Parallel2D.ApplyAngularImpulse(_body2D, impulse);
        }

        //============================== IParallelRigidBody ==============================
        public void OnParallelCollisionEnter(PCollision2D collision)
        {
            foreach (IParallelCollision2D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionEnter2D(collision);
            }
        }

        public void OnParallelCollisionStay(PCollision2D collision)
        {
            foreach (IParallelCollision2D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionStay2D(collision);
            }
        }

        public void OnParallelCollisionExit(PCollision2D collision)
        {
            foreach (IParallelCollision2D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionExit2D(collision);
            }
        }

        public void OnParallelTriggerEnter(IParallelRigidbody2D other)
        {
            foreach (IParallelTrigger2D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerEnter2D(other as ParallelRigidbody2D);
            }
        }

        public void OnParallelTriggerStay(IParallelRigidbody2D other)
        {
            foreach (IParallelTrigger2D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerStay2D(other as ParallelRigidbody2D);
            }
        }

        public void OnParallelTriggerExit(IParallelRigidbody2D other)
        {
            foreach (IParallelTrigger2D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerExit2D(other as ParallelRigidbody2D);
            }
        }

        public void OnTransformUpdated()
        {
            pTransform._internal_WriteTranform((Fix64Vec3)_body2D.position, new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.RadToDeg(_body2D.angle)));
        }

        public void Step(Fix64 timeStep)
        {
            foreach(IParallelFixedUpdate parallelFixedUpdate in parallelFixedUpdates)
            {
                parallelFixedUpdate.ParallelFixedUpdate(timeStep);
            }
        }

        //============================== Unity Events ==============================
        void Awake()
        {
            ParallelPhysicsController2D pSettings = FindObjectOfType<ParallelPhysicsController2D>();

            if(pSettings == null)
            {
                return;
            }

            pSettings.InitIfNecessary();

            parallelFixedUpdates = GetComponents<IParallelFixedUpdate>();
            parallelCollisions = GetComponents<IParallelCollision2D>();
            parallelTriggers = GetComponents<IParallelTrigger2D>();

            pTransform.ImportFromUnity();

            colliders = GetComponentsInChildren<ParallelCollider2D>();

            _body2D = Parallel2D.AddBody(
                                                (int)bodyType,
                                                (Fix64Vec2)pTransform.position,
                                                pTransform.rotation.GetZAngle(),
                                                linearDampling,
                                                angularDamping,
                                                fixedRotation,
                                                gravityScale,
                                                this);

            _bodyID = _body2D.BodyID;

            foreach (ParallelCollider2D collider in colliders)
            {   
                collider.SetRootGameObject(gameObject);
                PShape2D shape = collider.CreateShape(gameObject);

                if (shape == null)
                {
                    Debug.LogError("Failed to create collider shape");
                    continue;
                }

                PFixture2D fixture2D = Parallel2D.AddFixture(_body2D, shape, (Fix64)1);

                collider.ReceiveFixture(fixture2D);
            }
        }

        //============================== IReplayable ==============================
        public void Save(uint step)
        {
            _body2D.SaveExport(step);
        }

        public bool Load(uint step)
        {
            bool result = _body2D.LoadSavedExport(step);
            pTransform._internal_WriteTranform((Fix64Vec3)_body2D.position, new Fix64Vec3(Fix64.zero, Fix64.zero, Fix64.RadToDeg(_body2D.angle)));

            return result;
        }
    }
}