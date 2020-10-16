using UnityEngine;
using Parallel;
using System;

namespace Parallel
{
    public class ParallelPhysicsController2D : MonoBehaviour
    {
        public Parallel.LogLevel LoggingLevel;
        public bool autoUpdate = true;

        public float fixedUpdateTime;
        public Fix64 fixedUpdateTime64 = Fix64.FromDivision(2, 100);

        public int velocityIteration = 4;
        public int positionIteration = 2;

        public UInt16 bodyExportSize = 128;

        bool _initialized = false;

        public void InitIfNecessary()
        {
            if(!_initialized)
            {
                _initialized = true;
                Parallel2D.SetLoggingLevel(LoggingLevel);
                Parallel2D.bodyExportSize = bodyExportSize;
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
                Step();
                ExcuteUserCallbacks();
                ExcuteUserFixedUpdate();
            }
        }

        public void Step()
        {
            Parallel2D.Step(fixedUpdateTime64, velocityIteration, positionIteration); 
        }

        public void ExcuteUserCallbacks()
        {
            Parallel2D.ExcuteUserCallbacks(fixedUpdateTime64);
        }

        public void ExcuteUserFixedUpdate()
        {
            Parallel2D.ExcuteUserFixedUpdate(fixedUpdateTime64);
        }

        public void Update()
        {
            if (!autoUpdate)
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
