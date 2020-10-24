using UnityEngine;

namespace Parallel
{
    public delegate void TransformScaleUpdated();

    /// <summary>
    /// Position, rotation, and scale of an object.
    /// </summary>
    [ExecuteInEditMode]
    public class ParallelTransform : MonoBehaviour
    {
        public bool Interpolation = true;
        Vector3 _interpolateStartPosition;
        float _interpolateProgress;

        [SerializeField]
        Fix64Vec3 _localPosition = Fix64Vec3.zero;
        [SerializeField]
        Fix64Quat _localRotation = Fix64Quat.identity;
        [SerializeField]
        Fix64Vec3 _localEularAngles = Fix64Vec3.zero;
        [SerializeField]
        Fix64Vec3 _localScale = Fix64Vec3.one;

        //
        ParallelRigidbody2D _rigidbody2D;
        ParallelRigidbody3D _rigidbody3D;

        bool _quatReady = true;
        bool _eularReady = true;

        Fix64Vec3 _internalLocalEularAngles
        {
            get
            {
                if(_eularReady)
                {
                    return _localEularAngles;
                }
                else
                {
                    _localEularAngles = _localRotation.EulerAngles();
                    _eularReady = true;
                    return _localEularAngles;
                }
            }
            set
            {
                _localEularAngles = value;
                _eularReady = true;
                _quatReady = false;
            }
        }

        Fix64Quat _internalLocalRotation
        {
            get
            {
                if(_quatReady)
                {
                    return _localRotation;
                }
                else
                {
                    _localRotation = Fix64Quat.FromEulerAngles(_localEularAngles);
                    _quatReady = true;
                    return _localRotation;
                }
            }
            set
            {
                _localRotation = value;
                _quatReady = true;
                _eularReady = false;
            }
        }

        private void Reset()
        {
            ImportFromUnity();
            ExportToUnity();
        }

        public ParallelTransform parent
        {
            get
            {
                Transform uParent = unityParent;
                if(uParent == null)
                {
                    return null;
                }
                ParallelTransform parentParallelTransform = uParent.gameObject.GetComponent<ParallelTransform>();
                return parentParallelTransform;
            }
        }

        public Transform unityParent
        {
            get
            {
                Transform p = transform.parent;
                return p;
            }
        }

        Fix64Mat4x4 matrix
        {
            get
            {
                Fix64Mat4x4 m = Fix64Mat4x4.FromTRS(_localPosition, _internalLocalRotation, _localScale);
                return m;
            }
        }

        Fix64Mat4x4 matrixUnscaled
        {
            get
            {
                Fix64Mat4x4 m = Fix64Mat4x4.FromTRS(_localPosition, _internalLocalRotation, Fix64Vec3.one);
                return m;
            }
        }

        Fix64Mat4x4 inverseMatrix
        {
            get
            {
                Fix64Vec3 inverseTranslation = -_localPosition;
                Fix64Quat inverseRotation = Fix64Quat.Inverse(_internalLocalRotation);
                Fix64Vec3 inverseScale = Fix64.one / _localScale;

                Fix64Mat4x4 m = Fix64Mat4x4.FromTRS(inverseTranslation, inverseRotation, inverseScale);
                return m;
            }
        }

        Fix64Quat inverseRotation
        {
            get
            {
                Fix64Quat inverseRotation = Fix64Quat.Inverse(_internalLocalRotation);
                return inverseRotation;
            }
        }


        public Fix64Mat4x4 localToWorldMatrix
        {
            get
            {
                if (parent == null)
                {
                    return matrix;
                }

                Fix64Mat4x4 parentLocalToWorld = parent.localToWorldMatrix;
                Fix64Mat4x4 r = parentLocalToWorld * matrix;
                return r;
            }
        }

        public Fix64Mat4x4 localToWorldMatrixUnscaled
        {
            get
            {
                if (parent == null)
                {
                    return matrixUnscaled;
                }

                Fix64Mat4x4 parentLocalToWorld = parent.localToWorldMatrixUnscaled;
                Fix64Mat4x4 r = parentLocalToWorld * matrixUnscaled;
                return r;
            }
        }

        public Fix64Mat4x4 worldToLocalMatrix
        {
            get
            {
                if(parent == null)
                {
                    return inverseMatrix;
                }

                Fix64Mat4x4 parentWorldToLocal = parent.worldToLocalMatrix;
                Fix64Mat4x4 r = parentWorldToLocal * inverseMatrix;
                return r;
            }
        }

        void Update()
        {
            //only import from unity in editing mode
            if(!Application.isPlaying)
            {
                ImportFromUnity();
            }
            else
            {
                if(Interpolation)
                {
                    float increment = Time.deltaTime / Time.fixedDeltaTime;
                    _interpolateProgress = Mathf.Min(1.0f, _interpolateProgress + increment);
                    transform.localPosition = Vector3.Lerp(_interpolateStartPosition, (Vector3)_localPosition, _interpolateProgress);
                }
            }
        }

        /// <summary>
        /// Exports the fixed point values to the Unity Transform Component
        /// </summary>
        private void ExportToUnity()
        {
            if(!Application.isPlaying || !Interpolation)
            {
                transform.localPosition = (Vector3)_localPosition;
                _interpolateProgress = 1;
            }
            
            if (_eularReady)
            {
                transform.localEulerAngles = (Vector3)_localEularAngles;
            }
            else
            {
                transform.localRotation = (Quaternion)_localRotation;
            }
            transform.localScale = (Vector3)_localScale;
        }

        /// <summary>
        /// Imports the floating point values from the Unity Transform Component
        /// NOT deterministic
        /// </summary>
        public void ImportFromUnity()
        {
            //Debug.LogWarning("ImportFromUnity");
            _localPosition = (Fix64Vec3)transform.localPosition;
            _localRotation = (Fix64Quat)transform.localRotation;
            _localEularAngles = (Fix64Vec3)transform.localEulerAngles;
            _localScale = (Fix64Vec3)transform.localScale;
            _interpolateStartPosition = transform.localPosition;
        }

        public Fix64Vec3 position
        {
            get
            {
                if(parent == null)
                {
                    return _localPosition;
                }

                Fix64Mat4x4 m = parent.localToWorldMatrix;
                Fix64Vec3 p = m.MultiplyPoint3x4(_localPosition);
                return p;
            }
            set
            {
                if (parent == null)
                {
                    localPosition = value;
                    return;
                }

                Fix64Mat4x4 m = parent.worldToLocalMatrix;
                Fix64Vec3 p = m.MultiplyPoint3x4(value);

                localPosition = p;
            }
        }

        public Fix64Vec3 localPosition
        {
            get
            {
                return _localPosition;
            }
            set
            {
                if (_localPosition == value)
                {
                    return;
                }

                //set unity transform to the previous parallel transform position
                if (Interpolation)
                {
                    transform.localPosition = (Vector3)_localPosition;
                    _interpolateStartPosition = transform.localPosition;
                    _interpolateProgress = 0;
                }

                _localPosition = value;

                UpdateRigidbodyTransform();
                ExportToUnity();
            }
        }

        void UpdateRigidbodyTransform()
        {
                if(_rigidbody2D == null)
                {
                    _rigidbody2D = GetComponent<ParallelRigidbody2D>();
                }

                if(_rigidbody3D == null)
                {
                    _rigidbody3D = GetComponent<ParallelRigidbody3D>();
                }

                if(_rigidbody2D != null)
                {
                    Parallel2D.UpdateBodyTransForm(_rigidbody2D._body2D, (Fix64Vec2)_localPosition, Fix64.DegToRad(_internalLocalEularAngles.z));
                }

                if(_rigidbody3D != null)
                {
                    Parallel3D.UpdateBodyTransForm(_rigidbody3D._body3D, _localPosition, _internalLocalRotation);
                }
        }


        /// <summary>
        ///   <para>The rotation as Euler angles in degrees.</para>
        /// </summary>
        public Fix64Vec3 localEulerAngles
        {
            get
            {
                return _internalLocalEularAngles;
            }
            set
            {
                if(_internalLocalEularAngles == value)
                {
                    return;
                }

                _internalLocalEularAngles = value;
                UpdateRigidbodyTransform();
                ExportToUnity();
            } 
        }

        public Fix64Vec3 eulerAngles
        {
            get
            {
                if (parent == null)
                {
                    return localEulerAngles;
                }

                Fix64Vec3 parentAngles = parent.eulerAngles;
                Fix64Vec3 worldAngles = parentAngles + _internalLocalEularAngles;
                return worldAngles;
            }
            set
            {
                if(parent == null)
                {
                    localEulerAngles = value;
                    return;
                }

                //expensive
                Fix64Quat invRotP = parent.inverseRotation;
                Fix64Quat localRot = invRotP * Fix64Quat.FromEulerAngles(value);
                localRotation = localRot;
            }
        }

        public Fix64Quat localRotation
        {
            get
            {
                return _internalLocalRotation;
            }
            set
            {
                if (_internalLocalRotation == value)
                {
                    return;
                }

                _internalLocalRotation = value;
                UpdateRigidbodyTransform();
                ExportToUnity();
            }
        }

        public Fix64Quat rotation
        {
            get
            {
                if (parent == null)
                {
                    return _internalLocalRotation;
                }

                Fix64Quat rotP = parent.rotation;
                Fix64Quat worldRot = rotP * _internalLocalRotation;
                return worldRot;
            }
            set
            {
                if(parent == null)
                {
                    localRotation = value;
                    return;
                }

                Fix64Quat invRotP = parent.inverseRotation;
                Fix64Quat localRot = invRotP * value;
                localRotation = localRot;
            }
        }


        /// <summary>
        ///   <para>The scale of the transform relative to the parent. won't change collider dimensions in children GameObjects</para>
        /// </summary>
        public Fix64Vec3 localScale
        {
            get
            {
                return _localScale;
            }
            set
            {
                if (_localScale == value)
                {
                    return;
                }

                _localScale = value;
                //Todo: update collider dimensions
                ExportToUnity();
            }
        }

        public Fix64Vec3 right
        {
            get
            {
                return rotation * Fix64Vec3.right;
            }
        }

        public Fix64Vec3 up
        {
            get
            {
                return rotation * Fix64Vec3.up;
            }
        }

        public Fix64Vec3 forward
        {
            get
            {
                return rotation * Fix64Vec3.forward;
            }
        }

        //transform helper

        //Transforms position from local space to world space.
        public Fix64Vec3 TransformPoint(Fix64Vec3 position)
        {
            Fix64Mat4x4 m = localToWorldMatrix;
            Fix64Vec3 p = m.MultiplyPoint3x4(position);
            return p;
        }

        public Fix64Vec3 TransformPointUnscaled(Fix64Vec3 position)
        {
            Fix64Mat4x4 m = localToWorldMatrixUnscaled;
            Fix64Vec3 p = m.MultiplyPoint3x4(position);
            return p;
        }

        //Transforms position from world space to local space.
        public Fix64Vec3 InverseTransformPoint(Fix64Vec3 position)
        {
            Fix64Mat4x4 m = worldToLocalMatrix;
            Fix64Vec3 p = m.MultiplyPoint3x4(position);
            return p;
        }

        //Transforms direction from local space to world space.
        public Fix64Vec3 TransformDirection(Fix64Vec3 direction)
        {
            Fix64Mat4x4 m = localToWorldMatrix;
            Fix64Vec3 p = m.MultiplyVector(direction);
            return p;
        }

        public Fix64Vec3 TransformDirectionUnscaled(Fix64Vec3 direction)
        {
            Fix64Mat4x4 m = localToWorldMatrixUnscaled;
            Fix64Vec3 p = m.MultiplyVector(direction);
            return p;
        }

        //Transforms direction from world space to local space.
        public Fix64Vec3 InverseTransfromDirction(Fix64Vec3 direction)
        {
            Fix64Mat4x4 m = worldToLocalMatrix;
            Fix64Vec3 p = m.MultiplyVector(direction);
            return p;
        }

        //rotate in degree
        public void RotateInWorldSpace(Fix64Vec3 eulers)
        {
            //Rotation = Fix64Quat.FromEulerAngles(eulers) * Rotation;
            rotation = rotation * Fix64Quat.FromEulerAngles(eulers);
        }

        public void RotateInLocalSpace(Fix64Vec3 eulers)
        {
            //Rotation = Fix64Quat.FromEulerAngles(eulers) * Rotation;
            localRotation = localRotation * Fix64Quat.FromEulerAngles(eulers);
        }

        //should only be used by the attached rigidbody to update the transform
        //IMPORTATNT: all the internal transform data are in world space and should only be called on the root GameObject
        internal void _internal_WriteTranform(Fix64Vec3 position, Fix64Vec3 eulerAngles)
        {
            //set unity transform to the previous parallel transform position
            if (Interpolation)
            {
                transform.localPosition = (Vector3)_localPosition;
                _interpolateStartPosition = transform.localPosition;
                _interpolateProgress = 0;
            }


            _localPosition = position;
            _internalLocalEularAngles = eulerAngles;

            ExportToUnity();
        }

        internal void _internal_WriteTranform(Fix64Vec3 position, Fix64Quat rotation)
        {
            //set unity transform to the previous parallel transform position
            if(Interpolation)
            {
                transform.localPosition = (Vector3)_localPosition;
                _interpolateStartPosition = transform.localPosition;
                _interpolateProgress = 0;
            }

            _localPosition = position;
            _internalLocalRotation = rotation;

            ExportToUnity();
        }

        internal void _internal_ExportToUnity()
        {
            if(!Interpolation)
            {
                transform.localPosition = (Vector3)_localPosition;
            }
            
            if (_eularReady)
            {
                transform.localEulerAngles = (Vector3)_localEularAngles;
            }
            else
            {
                transform.localRotation = (Quaternion)_localRotation;
            }
            transform.localScale = (Vector3)_localScale;
        }
    }
}
