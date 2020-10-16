using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class TriggerEvents3D : MonoBehaviour, IParallelTrigger3D
    {
        public void OnParallelTriggerEnter3D(ParallelRigidbody3D other)
        {
            Debug.Log($"OnParallelTriggerEnter3D {other.gameObject.name}");
        }

        public void OnParallelTriggerExit3D(ParallelRigidbody3D other)
        {
            Debug.Log($"OnParallelTriggerExit3D {other.gameObject.name}");
        }

        public void OnParallelTriggerStay3D(ParallelRigidbody3D other)
        {
            Debug.Log($"OnParallelTriggerStay3D {other.gameObject.name}");
        }
    }
}
