using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class CollisionEvents3D : MonoBehaviour, IParallelCollision3D
    {
        public void OnParallelCollisionEnter3D(PCollision3D collision)
        {
            ParallelRigidbody3D rb = collision.otherRigidbody as ParallelRigidbody3D;
            Debug.Log($"OnParallelCollisionEnter3D {rb.gameObject.name}");
        }

        public void OnParallelCollisionExit3D(PCollision3D collision)
        {
            ParallelRigidbody3D rb = collision.otherRigidbody as ParallelRigidbody3D;
            Debug.Log($"OnParallelCollisionExit3D {rb.gameObject.name}");
        }

        public void OnParallelCollisionStay3D(PCollision3D collision)
        {
            ParallelRigidbody3D rb = collision.otherRigidbody as ParallelRigidbody3D;
            Debug.Log($"OnParallelCollisionStay3D {rb.gameObject.name}");
        }
    }
}
