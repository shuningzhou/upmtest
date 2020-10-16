using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

public class Rotate : MonoBehaviour, IParallelFixedUpdate
{
    ParallelTransform pTransform;
    public float rotateSpeed;

    public void ParallelFixedUpdate(Fix64 deltaTime)
    {
        pTransform.RotateInWorldSpace(Fix64Vec3.forward * (Fix64)rotateSpeed * deltaTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        pTransform = GetComponent<ParallelTransform>();
    }
}
