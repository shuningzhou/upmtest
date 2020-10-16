using System;
namespace Parallel
{
    public struct PBodyExport2D
    {
        public Fix64Vec2 linearVelocity;
        public Fix64 angularVelocity;

        public Fix64Vec2 position;
        public Fix64 angle;
    }

    [Serializable]
    public class PBody2D : NativeObject
    {
        internal PBodyExport2D[] bodyExports;
        UInt16 _exportsCapacity;
        int _maxIndex;
        int _index;
        UInt32 _maxStep;
        UInt32 _minStep;
        bool _initialized;

        //
        public Fix64Vec2 position;
        public Fix64 angle;

        internal Fix64Vec2 linearVelocity;
        internal Fix64 angularVelocity;

        public UInt16 BodyID { get; private set; }

        public IParallelRigidbody2D RigidBody { get; private set; }

        public PBody2D(IntPtr intPtr, UInt16 bodyID, IParallelRigidbody2D rigidBody, UInt16 exportSize) : base(intPtr)
        {
            BodyID = bodyID;
            RigidBody = rigidBody;

            bodyExports = new PBodyExport2D[exportSize];
            _exportsCapacity = exportSize;

            _index = 0;
            _maxIndex = 0;
            _maxStep = 0;
            _minStep = 0;

            _initialized = false;
        }

        public void ReadNative()
        {
            Parallel2D.ReadNativeBody(this);
            RigidBody.OnTransformUpdated();
        }

        public void Step(Fix64 time)
        {
            RigidBody.Step(time);
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

                PBodyExport2D export = bodyExports[_index];

                export.linearVelocity = linearVelocity;
                export.angularVelocity = angularVelocity;
                export.position = position;
                export.angle = angle;

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

                PBodyExport2D export = bodyExports[_index];

                export.linearVelocity = linearVelocity;
                export.angularVelocity = angularVelocity;
                export.position = position;
                export.angle = angle;

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

            PBodyExport2D export = bodyExports[index];

            linearVelocity = export.linearVelocity;
            angularVelocity = export.angularVelocity;
            position = export.position;
            angle = export.angle;

            Parallel2D.UpdateBodyTransForm(this, position, angle);
            Parallel2D.UpdateBodyVelocity(this, linearVelocity, angularVelocity);

            return true;
        }
    }
}
