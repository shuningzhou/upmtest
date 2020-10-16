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
            Debug.Log($"OnParallelCollisionEnter3D {collision.otherRigidbody.gameObject.name}");
        }

        public void OnParallelCollisionExit3D(PCollision3D collision)
        {
            Debug.Log($"OnParallelCollisionExit3D {collision.otherRigidbody.gameObject.name}");
        }

        public void OnParallelCollisionStay3D(PCollision3D collision)
        {
            Debug.Log($"OnParallelCollisionStay3D {collision.otherRigidbody.gameObject.name}");
        }
    }
}
