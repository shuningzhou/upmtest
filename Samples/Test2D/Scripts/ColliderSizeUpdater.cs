using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Parallel;

namespace Parallel.Sample
{
    public class ColliderSizeUpdater : MonoBehaviour
    {
        ParallelBoxCollider _boxCollider;
        Fix64 scale = Fix64.FromDivision(12, 10);

        // Start is called before the first frame update
        void Start()
        {
            _boxCollider = GetComponent<ParallelBoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Plus))
            {
                _boxCollider.UpdateShape(_boxCollider.size * scale);
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                _boxCollider.UpdateShape(_boxCollider.size / scale);
            }
        }
    }
}