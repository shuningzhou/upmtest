using System;
namespace Parallel
{
    public struct PBodyExport3D
    {
        public Fix64Vec3 linearVelocity;
        public Fix64Vec3 angularVelocity;

        public Fix64Vec3 position;
        public Fix64Quat orientation;
        public Fix64Quat orientation0;

        public bool awake;
        public Fix64 sleepTime;
    }

    public class PBody3D : NativeObject
    {
        internal PBodyExport3D[] bodyExports;
        UInt16 _exportsCapacity;
        int _maxIndex;
        int _index;
        UInt32 _maxStep;
        UInt32 _minStep;
        bool _initialized;
        public bool awake;
        public Fix64 sleepTime;
        public Fix64Vec3 position;
        public Fix64Quat orientation;
        public Fix64Quat orientation0;

        public Fix64Vec3 linearVelocity;
        public Fix64Vec3 angularVelocity;

        public UInt16 BodyID { get; private set; }

        public ParallelRigidbody3D RigidBody { get; private set; }
        public bool IsAwake
        {
            get{
                return awake;
            }
        }
        public PBody3D(IntPtr intPtr, UInt16 bodyID, ParallelRigidbody3D rigidBody, UInt16 exportSize) : base(intPtr)
        {
            BodyID = bodyID;
            RigidBody = rigidBody;

            bodyExports = new PBodyExport3D[exportSize];
            _exportsCapacity = exportSize;

            _index = 0;
            _maxIndex = 0;
            _maxStep = 0;
            _minStep = 0;
            awake = true;
            _initialized = false;
        }

        public void ReadNative()
        {
            Parallel3D.ReadNativeBody(this);
        }

        public void SaveExport(UInt32 step)
        {
            if (step < _minStep)
            {
                _initialized = false;
            }

            if (!_initialized)
            {
                _maxStep = step;
                _initialized = true;
                _maxIndex = 0;
                _index = 0;
            }

            if (step == _maxStep + 1 || _index == 0)
            {
                if (_index == _exportsCapacity)
                {
                    _index = 0;
                }

                PBodyExport3D export = bodyExports[_index];

                export.linearVelocity = linearVelocity;
                export.angularVelocity = angularVelocity;
                export.position = position;
                export.orientation = orientation;
                export.orientation0 = orientation0;
                export.awake = awake;
                export.sleepTime = sleepTime;

                bodyExports[_index] = export;

                _maxIndex = _index;
                _maxStep = step;

                if(_maxStep - _minStep >= _exportsCapacity)
                {
                    _minStep = _maxStep - _exportsCapacity + 1;
                }

                _index++;
            }
            else
            {
                int index = CalculateExportIndex(step);
                _index = index;

                PBodyExport3D export = bodyExports[_index];

                export.linearVelocity = linearVelocity;
                export.angularVelocity = angularVelocity;
                export.position = position;
                export.orientation = orientation;
                export.orientation0 = orientation0;
                export.awake = awake;
                export.sleepTime = sleepTime;

                bodyExports[_index] = export;

                _maxIndex = _index;
                _maxStep = step;

                _index++;
            }
        }

        public int CalculateExportIndex(UInt32 step)
        {
            if (step > _maxStep)
            {
                return -1;
            }

            if (step < _minStep)
            {
                return -1;
            }

            UInt32 diff = _maxStep - step;
            int index = (int)(_maxIndex - diff);
            if(index < 0)
            {
                index = _exportsCapacity + index;
            }

            return index;
        }

        public bool LoadSavedExport(UInt32 step)
        {
            if(step > _maxStep)
            {
                return false;
            }

            if(step < _minStep)
            {
                return false;
            }

            int index = CalculateExportIndex(step);

            PBodyExport3D export = bodyExports[index];

            linearVelocity = export.linearVelocity;
            angularVelocity = export.angularVelocity;
            position = export.position;
            orientation = export.orientation;
            orientation0 = export.orientation0;
            awake = export.awake;
            sleepTime = export.sleepTime;

            Parallel3D.UpdateBodyTransformForRollback(this, position, orientation, orientation0);
            Parallel3D.UpdateBodyVelocity(this, linearVelocity, angularVelocity);
            Parallel3D.SetAwakeForRollback(this, awake, sleepTime);
            return true;
        }
    }
}
