using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class Raycast : MonoBehaviour
    {
        Fix64Vec2 raycastPoint;
        Fix64Vec2 raycastNormal;

        public float rotateSpeed = 1;
        public float raycastLength = 10.0f;
        public float gizmoSize = 0.1f;
        public LayerMask layerMask;

        bool started;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(transform.position, new Vector3(0.1f, 0.1f, 0.1f));

            if (started)
            {
                Gizmos.DrawLine(transform.position, (Vector2)raycastPoint);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere((Vector2)raycastPoint, gizmoSize);

                Vector2 direction = (Vector2)raycastNormal * 1f;
                Gizmos.DrawRay((Vector2)raycastPoint, direction);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

            PRaycastHit2D raycastHit2D;

            bool hit = false;


            Fix64Vec2 start = (Fix64Vec2)transform.position;
            Fix64Vec2 end = start + (Fix64)raycastLength * (Fix64Vec2)transform.right;

            hit = Parallel2D.RayCast(start, end, layerMask, out raycastHit2D);

            if (hit)
            {
                raycastPoint = raycastHit2D.point;
                raycastNormal = raycastHit2D.normal;
            }
            else
            {
                raycastPoint = end;
            }
        }
    }
}
