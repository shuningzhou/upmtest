using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class CollisionEvents : MonoBehaviour, IParallelCollision2D
    {
        PContactPoints2D contactPoints;

        public void OnParallelCollisionEnter2D(PCollision2D collision)
        {
            ParallelRigidbody2D rb = collision.otherRigidbody as ParallelRigidbody2D;
            Debug.Log($"OnParallelCollisionEnter2D {rb.gameObject.name}");

            collision.GetContactPoints(ref contactPoints);

            Debug.Log($"OnParallelCollisionEnter2D {contactPoints}");
        }

        public void OnParallelCollisionExit2D(PCollision2D collision)
        {
            ParallelRigidbody2D rb = collision.otherRigidbody as ParallelRigidbody2D;
            Debug.Log($"OnParallelCollisionExit2D {rb.gameObject.name}");

            collision.GetContactPoints(ref contactPoints);

            Debug.Log($"OnParallelCollisionExit2D {contactPoints}");
        }

        public void OnParallelCollisionStay2D(PCollision2D collision)
        {
            ParallelRigidbody2D rb = collision.otherRigidbody as ParallelRigidbody2D;
            Debug.Log($"OnParallelCollisionStay2D {rb.gameObject.name}");

            collision.GetContactPoints(ref contactPoints);

            Debug.Log($"OnParallelCollisionStay2D {contactPoints}");
        }
    }
}