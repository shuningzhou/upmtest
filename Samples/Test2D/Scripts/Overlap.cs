using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class Overlap : MonoBehaviour
{
    public float circleRadius = 1f;
    public float gizmoSize = 0.1f;
    public LayerMask layerMask;

    PShapeOverlapResult2D result;
    bool _started;
    // Start is called before the first frame update
    void Start()
    {
        _started = true;
        result = new PShapeOverlapResult2D();
    }

    private void OnDrawGizmos()
    {
        if(_started)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, circleRadius);

            if (result.count > 0)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < result.count; i++)
                {
                    ParallelRigidbody2D rigidBody2D = result.rigidbodies[i] as ParallelRigidbody2D;
                    Gizmos.DrawWireSphere(rigidBody2D.transform.position, gizmoSize);
                }
            }
        }
    }

    void Update()
    {
        Fix64Vec2 center = (Fix64Vec2)transform.position;
        Fix64 radius = (Fix64)circleRadius;

        Parallel2D.OverlapCircle(center, radius, layerMask, result);
    }
}
