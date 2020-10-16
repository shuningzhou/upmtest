using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class Overlap3D : MonoBehaviour
{
    public float sphereRadius = 1f;
    public float gizmoSize = 0.1f;
    public LayerMask layerMask;
    PShapeOverlapResult3D result;
    bool _started;
    // Start is called before the first frame update
    void Start()
    {
        _started = true;
        result = new PShapeOverlapResult3D();
    }

    private void OnDrawGizmos()
    {
        if (_started)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sphereRadius);

            if (result.count > 0)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < result.count; i++)
                {
                    ParallelRigidbody3D rigidBody3D = result.rigidbodies[i];
                    Gizmos.DrawWireSphere(rigidBody3D.transform.position + Vector3.up * 2, gizmoSize);
                }
            }
        }
    }

    void Update()
    {
        Fix64Vec3 center = (Fix64Vec3)transform.position;
        Fix64 radius = (Fix64)sphereRadius;

        Parallel3D.OverlapSphere(center, radius, layerMask, result);
    }
}
