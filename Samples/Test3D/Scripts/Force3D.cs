using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class Force3D : MonoBehaviour, IParallelFixedUpdate
    {
        public float strength = 5;
        Fix64 _strength;
        public Fix64 hortizontal = Fix64.zero;
        public Fix64 vertical = Fix64.zero;

        bool _fire;
        ParallelRigidbody3D _rigidbody;
        ParallelTransform _pTransform;

        public void ParallelFixedUpdate(Fix64 deltaTime)
        {
            Fix64Vec3 force = new Fix64Vec3(
                                    _strength * hortizontal,
                                    Fix64.zero,
                                    _strength * vertical);

            _rigidbody.ApplyForce(force);

        }

        // Start is called before the first frame update
        void Start()
        {
            _pTransform = GetComponent<ParallelTransform>();
            _rigidbody = GetComponent<ParallelRigidbody3D>();
            _strength = (Fix64)strength;
        }

        // Update is called once per frame
        void Update()
        {
            _strength = (Fix64)strength;
            hortizontal = (Fix64)Input.GetAxis("Horizontal");
            vertical = (Fix64)Input.GetAxis("Vertical");
        }
    }
}