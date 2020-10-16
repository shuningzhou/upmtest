using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;
using ParallelUnity;

public class SetPosition3D : MonoBehaviour, IParallelFixedUpdate
{
    public float speed = 2.0f;
    Fix64 _speed;
    public Fix64 hortizontal = Fix64.zero;
    public Fix64 vertical = Fix64.zero;

    ParallelTransform _pTransform;

    public void ParallelFixedUpdate(Fix64 deltaTime)
    {
        Fix64Vec3 delta = new Fix64Vec3(
                                _speed * deltaTime * hortizontal,
                                _speed * deltaTime * vertical,
                                Fix64.zero);

        _pTransform.position += delta;

    }

    // Start is called before the first frame update
    void Start()
    {
        _pTransform = GetComponent<ParallelTransform>();
        _speed = (Fix64)speed;
    }

    // Update is called once per frame
    void Update()
    {
        _speed = (Fix64)speed;
        hortizontal = (Fix64)Input.GetAxis("Horizontal");
        vertical = (Fix64)Input.GetAxis("Vertical");
    }
}