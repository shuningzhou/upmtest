using UnityEngine;
using Parallel;
using System;

namespace Parallel
{
    public class ParallelPhysicsController2D : MonoBehaviour
    {
        public Parallel.LogLevel LoggingLevel;
        public bool autoUpdate = true;
        public Fix64 fixedUpdateTime = Fix64.FromDivision(2, 100);
        public int velocityIteration = 4;

        public Fix64Vec2 gravity = new Fix64Vec2(Fix64.zero, Fix64.FromDivision(-98, 10));
        public UInt16 bodyExportSize = 128;
        bool _initialized = false;

        public void InitIfNecessary()
        {
            if(!_initialized)
            {
                _initialized = true;
                Parallel2D.gravity = gravity;
                Parallel2D.SetLoggingLevel(LoggingLevel);
                Parallel2D.bodyExportSize = bodyExportSize;
                Time.fixedDeltaTime = (float)fixedUpdateTime;
            }
        }

        private void Awake()
        {
            InitIfNecessary();
        }

        private void OnDestroy()
        {
            Parallel2D.CleanUp();
        }

        private void FixedUpdate()
        {
            if(autoUpdate)
            {
                Step(fixedUpdateTime);
                ExcuteUserCallbacks(fixedUpdateTime);
                ExcuteUserFixedUpdate(fixedUpdateTime);
            }
        }

        public void Step(Fix64 deltaTime)
        {
            Parallel2D.Step(deltaTime, velocityIteration, 1); 
        }

        public void ExcuteUserCallbacks(Fix64 deltaTime)
        {
            Parallel2D.ExcuteUserCallbacks(deltaTime);
        }

        public void ExcuteUserFixedUpdate(Fix64 deltaTime)
        {
            Parallel2D.ExcuteUserFixedUpdate(deltaTime);
        }

        public void Update()
        {
            if (!autoUpdate)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Step(fixedUpdateTime);
                    ExcuteUserCallbacks(fixedUpdateTime);
                }
            }
        }

        public void UpdateContacts()
        {
            Parallel2D.UpdateContacts();
        }

        public void PrepareExternalContactData()
        {
            Parallel2D.PrepareExternalContactData();
        }

        public void ExportEngineState(PInternalState2D state)
        {
            Parallel2D.ExportEngineInternalState(state);
        }

        public void ApplyEngineState(PInternalState2D state)
        {
            Parallel2D.AppleEngineInternalState(state);
        }
    }
}
