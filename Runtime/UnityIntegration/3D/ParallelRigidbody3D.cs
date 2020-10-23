using UnityEngine;

namespace Parallel
{
    [RequireComponent(typeof(ParallelTransform))]
    public class ParallelRigidbody3D : MonoBehaviour, IParallelRigidbody3D, IReplayable
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
        IParallelCollision3D[] parallelCollisions = new IParallelCollision3D[0];
        IParallelTrigger3D[] parallelTriggers = new IParallelTrigger3D[0];

        ParallelCollider3D[] colliders = new ParallelCollider3D[0];

        public PBody3D _body3D;
        [SerializeField]
        int _bodyID;
        [SerializeField]
        bool _awake;

        //============================== Body properties ==============================
        [SerializeField]
        BodyType _bodyType = Parallel.BodyType.Dynamic;

        [SerializeField]
        Fix64Vec3 _linearDamping = Fix64Vec3.zero;

        [SerializeField]
        Fix64Vec3 _angularDamping = new Fix64Vec3(Fix64.FromDivision(5, 100), Fix64.FromDivision(5, 100), Fix64.FromDivision(5, 100));

        [SerializeField]
        Fix64Vec3 _gravityScale = Fix64Vec3.one;

        [SerializeField]
        bool _fixedRotationX = false;
        [SerializeField]
        bool _fixedRotationY = false;
        [SerializeField]
        bool _fixedRotationZ = false;

        //only used when creating the body
        [SerializeField]
        bool _overideMassData = false;
        [SerializeField]
        Fix64 _mass = Fix64.one;
        [SerializeField]
        Fix64Vec3 _centerOfMass = Fix64Vec3.zero;

        public bool IsAwake
        {
            get
            {
                return _awake;
            }
        }

        public Fix64Vec3 COM
        {
            get
            {
                //TODO: get com from physics engine
                return _centerOfMass;
            }
        }

        public BodyType bodyType
        {
            get
            {
                return _bodyType;
            }
            set
            {
                _bodyType = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public Fix64Vec3 linearDamping
        {
            get
            {
                return _linearDamping;
            }
            set
            {
                _linearDamping = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public Fix64Vec3 angularDamping
        {
            get
            {
                return _angularDamping;
            }
            set
            {
                _angularDamping = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public Fix64Vec3 gravityScale
        {
            get
            {
                return _gravityScale;
            }
            set
            {
                _gravityScale = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public bool fixedRotationX
        {
            get
            {
                return _fixedRotationX;
            }
            set
            {
                _fixedRotationX = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public bool fixedRotationY
        {
            get
            {
                return _fixedRotationY;
            }
            set
            {
                _fixedRotationY = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        public bool fixedRotationZ
        {
            get
            {
                return _fixedRotationZ;
            }
            set
            {
                _fixedRotationZ = value;
                Parallel3D.UpdateBodyProperties(
                    _body3D, 
                    (int)bodyType, 
                    linearDamping, 
                    angularDamping, 
                    gravityScale, 
                    fixedRotationX, 
                    fixedRotationY, 
                    fixedRotationZ);
            }
        }

        //============================== Velocity ==============================
        public Fix64Vec3 LinearVelocity
        {
            get
            {
                return _body3D.linearVelocity;
            }
            set
            {
                _body3D.linearVelocity = value;
                Parallel3D.UpdateBodyVelocity(_body3D, LinearVelocity, AngularVelocity);
            }
        }

        public Fix64Vec3 AngularVelocity
        {
            get
            {
                return _body3D.angularVelocity;
            }
            set
            {
                _body3D.angularVelocity = value;
                Parallel3D.UpdateBodyVelocity(_body3D, LinearVelocity, AngularVelocity);
            }
        }

        //
        public Fix64Vec3 GetPointVelocity(Fix64Vec3 point)
        {
            return Parallel3D.GetPointVelocity(_body3D, point);
        }

        //============================== Force and Torque ==============================
        //Apply a force to the center of mass
        public void ApplyForce(Fix64Vec3 force)
        {
            Parallel3D.ApplyForceToCenter(_body3D, force);
        }

        //Apply a force at a world point
        public void ApplyForce(Fix64Vec3 force, Fix64Vec3 worldPoint)
        {
            Parallel3D.ApplyForce(_body3D, worldPoint, force);
        }

        //Apply an impulse to the center of mass. This immediately modifies the velocity.
        public void ApplyLinearImpulse(Fix64Vec3 impluse)
        {
            Parallel3D.ApplyLinearImpulseToCenter(_body3D, impluse);
        }

        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass.
        public void ApplyLinearImpluse(Fix64Vec3 impluse, Fix64Vec3 worldPoint)
        {
            Parallel3D.ApplyLinearImpulse(_body3D, worldPoint, impluse);
        }

        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// z-axis (out of the screen)
        public void ApplyTorque(Fix64Vec3 torque)
        {
            Parallel3D.ApplyTorque(_body3D, torque);
        }

        /// Apply an angular impulse. This immediately modifies the angular velocity
        public void ApplyAngularImpulse(Fix64Vec3 impulse)
        {
            Parallel3D.ApplyAngularImpulse(_body3D, impulse);
        }

        //============================== IParallelRigidBody ==============================
        public void OnParallelCollisionEnter(PCollision3D collision)
        {
            foreach (IParallelCollision3D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionEnter3D(collision);
            }
        }

        public void OnParallelCollisionStay(PCollision3D collision)
        {
            foreach (IParallelCollision3D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionStay3D(collision);
            }
        }

        public void OnParallelCollisionExit(PCollision3D collision)
        {
            foreach (IParallelCollision3D parallelCollision in parallelCollisions)
            {
                parallelCollision.OnParallelCollisionExit3D(collision);
            }
        }

        public void OnParallelTriggerEnter(IParallelRigidbody3D other)
        {
            foreach (IParallelTrigger3D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerEnter3D(other as ParallelRigidbody3D);
            }
        }

        public void OnParallelTriggerStay(IParallelRigidbody3D other)
        {
            foreach (IParallelTrigger3D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerStay3D(other as ParallelRigidbody3D);
            }
        }

        public void OnParallelTriggerExit(IParallelRigidbody3D other)
        {
            foreach (IParallelTrigger3D trigger in parallelTriggers)
            {
                trigger.OnParallelTriggerExit3D(other as ParallelRigidbody3D);
            }
        }

        public void OnTransformUpdated()
        {
            _awake = _body3D.IsAwake;
            pTransform._internal_WriteTranform(_body3D.position, _body3D.orientation);
        }

        public void Step(Fix64 timeStep)
        {
            foreach (IParallelFixedUpdate parallelFixedUpdate in parallelFixedUpdates)
            {
                parallelFixedUpdate.ParallelFixedUpdate(timeStep);
            }
        }

        //============================== Unity Events ==============================
        void Awake()
        {
            ParallelPhysicsController3D pSettings = FindObjectOfType<ParallelPhysicsController3D>();

            if (pSettings == null)
            {
                return;
            }

            pSettings.InitIfNecessary();

            parallelFixedUpdates = GetComponents<IParallelFixedUpdate>();
            parallelCollisions = GetComponents<IParallelCollision3D>();
            parallelTriggers = GetComponents<IParallelTrigger3D>();

            pTransform.ImportFromUnity();

            colliders = GetComponentsInChildren<ParallelCollider3D>();

            _body3D = Parallel3D.AddBody(
                                        (int)bodyType,
                                        pTransform.position,
                                        pTransform.rotation,
                                        linearDamping,
                                        angularDamping,
                                        gravityScale,
                                        fixedRotationX,
                                        fixedRotationY,
                                        fixedRotationZ,
                                        this);

            _bodyID = _body3D.BodyID;

            foreach (ParallelCollider3D collider in colliders)
            {
                PShape3D shape = collider.CreateShape(gameObject);

                if (shape == null)
                {
                    Debug.LogError("Failed to create collider shape");
                    continue;
                }

                PFixture3D fixture = Parallel3D.AddFixture(_body3D, shape, (Fix64)1);

                collider.ReceiveFixture(fixture);
            }

            if(_overideMassData)
            {
                //Parallel3D.UpdateMassData(_body3D, _mass, _centerOfMass);
                if (_centerOfMass != null)
                {
                    Fix64Vec3 com = _centerOfMass;
                    //Debug.Log(com);
                    Parallel3D.UpdateMassData(_body3D, _mass, com);


                }
                else
                {
                    Parallel3D.UpdateMass(_body3D, _mass);
                }
            }
        }

        public void AddToWorldForPathFinding()
        {
            if(_bodyType != BodyType.Static)
            {
                return;
            }

            ParallelCollider3D[] colliders = GetComponentsInChildren<ParallelCollider3D>();
            PBody3D body = Parallel3D.AddBody(
                            (int)bodyType,
                            pTransform.position,
                            pTransform.rotation,
                            linearDamping,
                            angularDamping,
                            gravityScale,
                            fixedRotationX,
                            fixedRotationY,
                            fixedRotationZ,
                            this);

            foreach (ParallelCollider3D collider in colliders)
            {
                PShape3D shape = collider.CreateShape(gameObject);

                if (shape == null)
                {
                    Debug.LogError("Failed to create collider shape");
                    continue;
                }

                Parallel3D.AddFixture(body, shape, (Fix64)1);
            }
        }

        //============================== IReplayable ==============================
        public void Save(uint step)
        {
            _body3D.SaveExport(step);
        }

        public bool Load(uint step)
        {
            bool result = _body3D.LoadSavedExport(step);
            //todo: velocity and awake
            pTransform._internal_WriteTranform(_body3D.position, _body3D.orientation);

            return result;
        }
    }
}