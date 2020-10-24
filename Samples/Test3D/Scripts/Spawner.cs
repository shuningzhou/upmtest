using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParallelUnity;
using Parallel;

public class Spawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public bool auto;
    public float interval = 1;
    float _interval;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }

        if(auto)
        {
            _interval += Time.deltaTime;

            if(_interval > interval)
            {
                _interval = 0;
                Spawn();
            }
        }
    }

    void Spawn()
    {
        int size = prefabs.Length;
        int index = Random.Range(0, size);
        GameObject go = Instantiate(prefabs[index], transform.position, Quaternion.identity);
        //ParallelTransform pTransform = go.GetComponent<ParallelTransform>();
        //if (pTransform != null)
        //{
        //    int randomSize = Random.Range(5, 30);
        //    pTransform.localScale = pTransform.localScale * Fix64.FromDivision(randomSize, 10);
        //    pTransform.position = (Fix64Vec3)transform.position;
        //}

        //PCollider3D collider = go.GetComponent<PCollider3D>();
        //collider.ShapeDirty = true;
    }
}
