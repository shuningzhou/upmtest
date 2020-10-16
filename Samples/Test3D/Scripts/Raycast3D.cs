using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class Raycast3D : MonoBehaviour
{
    Fix64Vec3 raycastPoint;
    Fix64Vec3 raycastNormal;

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
            Gizmos.DrawLine(transform.position, (Vector3)raycastPoint);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector3)raycastPoint, gizmoSize);

            Vector2 direction = (Vector3)raycastNormal * 1f;
            Gizmos.DrawRay((Vector3)raycastPoint, direction);
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
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        PRaycastHit3D raycastHit3D;

        bool hit = false;


        Fix64Vec3 start = (Fix64Vec3)transform.position;
        Fix64Vec3 end = start + (Fix64)raycastLength * (Fix64Vec3)transform.right;

        hit = Parallel3D.RayCast(start, end, layerMask, out raycastHit3D);

        if (hit)
        {
            raycastPoint = raycastHit3D.point;
            raycastNormal = raycastHit3D.normal;
        }
        else
        {
            raycastPoint = end;
        }
    }
}
