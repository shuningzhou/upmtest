using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class Force : MonoBehaviour, IParallelFixedUpdate
    {
        public float strength;
        Fix64 _strength;
        Fix64Vec2 _mousePos;
        bool _fire;
        ParallelRigidbody2D _rigidbody;
        ParallelTransform _transform;
        public GameObject cursor;
        Transform _cursorTransform;

        public void ParallelFixedUpdate(Fix64 deltaTime)
        {
            if (_fire)
            {
                Fix64Vec2 direction = _mousePos - (Fix64Vec2)_transform.position;
                direction = direction.normalized;
                Fix64Vec2 impulse = direction * _strength;
                _rigidbody.ApplyForce(impulse);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<ParallelRigidbody2D>();
            _transform = GetComponent<ParallelTransform>();
            _cursorTransform = Instantiate(cursor, Vector3.zero, Quaternion.identity).transform;
        }

        // Update is called once per frame
        void Update()
        {
            _strength = (Fix64)strength;

            if (Input.GetMouseButton(0))
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                    Input.mousePosition.y,
                                                                    -Camera.main.transform.position.z));
                Vector3 newPos = new Vector3(mouse.x, mouse.y, 0);
                _cursorTransform.position = newPos;
                _mousePos = (Fix64Vec2)newPos;
                _fire = true;
            }
            else
            {
                _fire = false;
            }
        }
    }
}