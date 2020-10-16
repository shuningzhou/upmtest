using System;
using UnityEngine;

namespace Parallel
{
    public class ParallelPhysicsController3D : MonoBehaviour
    {
        public Parallel.LogLevel LoggingLevel;
        public bool Debugging = false;
        public Fix64 fixedUpdateTime = Fix64.FromDivision(2, 100);
        public float speed = 1;
        public int velocityIteration = 4;
        public int positionIteration = 1;
        public bool allowSleep = true;
        public bool warmStart = true;
        public Fix64Vec3 gravity = new Fix64Vec3(Fix64.zero, Fix64.FromDivision(-98, 10), Fix64.zero);

        public UInt16 bodyExportSize = 128;

        bool _initialized = false;

        public void InitIfNecessary()
        {
            if (!_initialized)
            {
                _initialized = true;
                Parallel3D.gravity = gravity;
                Parallel3D.allowSleep = allowSleep;
                Parallel3D.warmStart = warmStart;
                Parallel3D.SetLoggingLevel(LoggingLevel);
                Parallel3D.bodyExportSize = bodyExportSize;
            }
        }

        private void Awake()
        {
            InitIfNecessary();
        }

        private void OnDestroy()
        {
            Parallel3D.CleanUp();
        }

        private void FixedUpdate()
        {
            if (!Debugging)
            {
                Step();
                ExcuteUserCallbacks();
            }
        }

        public void Step()
        {
            Parallel3D.Step(fixedUpdateTime, velocityIteration, positionIteration);
        }

        public void ExcuteUserCallbacks()
        {
            //using (new SProfiler($"==========ExcuteUserCallbacks========"))
            {
                Parallel3D.ExcuteUserCallbacks(fixedUpdateTime);
            }
        }

        public void Update()
        {
            //Time.fixedDeltaTime = 0.02f * (1 / speed);
            if (Debugging)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Step();
                    ExcuteUserCallbacks();
                }
            }
        }

        public void UpdateContacts()
        {
            Parallel3D.UpdateContacts();
        }

        public void PrepareExternalContactData()
        {
            Parallel3D.PrepareExternalContactData();
        }

        public void ExportEngineState(PInternalState3D state)
        {
            Parallel3D.ExportEngineInternalState(state);
        }

        public void AddWarmStart(PInternalState3D state)
        {
            Parallel3D.AddExternalContactWarmStartData(state);
        }

        public void PrepareExternalConvexCacheData()
        {
            Parallel3D.PrepareExternalConvexCacheData();
        }

        public void AddConvexCache(PInternalState3D state)
        {
            Parallel3D.AddExternalConvexCache(state);
        }
    }
}
