using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class TriggerEvents : MonoBehaviour, IParallelTrigger2D
{
    public void OnParallelTriggerEnter2D(ParallelRigidbody2D other)
    {
        Debug.Log($"OnParallelTriggerEnter2D {other.gameObject.name}");
    }

    public void OnParallelTriggerExit2D(ParallelRigidbody2D other)
    {
        Debug.Log($"OnParallelTriggerExit2D {other.gameObject.name}");
    }

    public void OnParallelTriggerStay2D(ParallelRigidbody2D other)
    {
        Debug.Log($"OnParallelTriggerStay2D {other.gameObject.name}");
    }
}
