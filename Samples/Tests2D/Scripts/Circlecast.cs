using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class Circlecast : MonoBehaviour
{
    Fix64Vec2 castPoint;
    Fix64Vec2 castNormal;
    Fix64Vec2 circleHitPosition;
    PShapecastHit2D hitInfo;

    public float rotateSpeed = 30;
    public float castRange = 10.0f;
    public float circleRadius = 0.5f;
    public float gizmoSize = 0.1f;
    public LayerMask layerMask;

    bool started;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(transform.position, new Vector3(gizmoSize, gizmoSize, gizmoSize));

        if (started)
        {
            Gizmos.DrawLine(transform.position, (Vector2)circleHitPosition);
            Gizmos.DrawSphere((Vector2)circleHitPosition, circleRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((Vector2)castPoint, gizmoSize);

            Vector2 direction = (Vector2)castNormal * 1f;
            Gizmos.DrawRay((Vector2)castPoint, direction);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        started = true;
        hitInfo = new PShapecastHit2D();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        bool hit = false;

        Fix64Vec3 start = (Fix64Vec3)transform.position;
        Fix64Vec3 movement = (Fix64)castRange * (Fix64Vec3)transform.right;
        Fix64 radius = (Fix64)circleRadius;
        Fix64Vec3 end = start + movement;

        hit = Parallel2D.CircleCast((Fix64Vec2)start, radius, (Fix64Vec2)movement, layerMask, ref hitInfo);

        if (hit)
        {
            castPoint = hitInfo.point;
            castNormal = hitInfo.normal;
            circleHitPosition = (Fix64Vec2)(start + hitInfo.fraction * movement);
        }
        else
        {
            castPoint = (Fix64Vec2)end;
            castNormal = Fix64Vec2.zero;
            circleHitPosition = castPoint;
        }
    }
}
