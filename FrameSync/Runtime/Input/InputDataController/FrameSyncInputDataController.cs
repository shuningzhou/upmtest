using Parallel;
using SWNetwork.Core;
using System;
using System.Collections;

namespace SWNetwork.FrameSync
{
    internal abstract class FrameSyncInputDataController
    {
        internal string _id = Guid.NewGuid().ToString();
        protected byte _bitOffset;

        public FrameSyncInputDataController(byte bitOffset)
        {
            _bitOffset = bitOffset;
        }

        public virtual void InputJustCopied(BitArray bits)
        {

        }

        public virtual void Export(BitArray bitArray)
        {
            throw new InvalidOperationException($"NetworkInputDataController WriteInputBuffer not implemented");
        }

        // bool
        public virtual bool GetBoolValue(BitArray bits)
        {
            throw new InvalidOperationException($"NetworkInputDataController GetBoolValue Unmatched type {GetType()}");
        }

        public virtual void SetValue(bool value)
        {
            throw new InvalidOperationException($"NetworkInputDataController SetValue float Unmatched type {GetType()}");
        }

        public virtual int GetIntValue(BitArray bits)
        {
            throw new InvalidOperationException($"NetworkInputDataController GetIntValue Unmatched type {GetType()}");
        }

        public virtual void SetValue(int value)
        {
            throw new InvalidOperationException($"NetworkInputDataController SetValue int Unmatched type {GetType()}");
        }

        // float
        public virtual Fix64 GetFloatValue(BitArray bits)
        {
            throw new InvalidOperationException($"NetworkInputDataController GetFloatValue Unmatched type {GetType()}");
        }

        public virtual void SetValue(Fix64 value)
        {
            throw new InvalidOperationException($"NetworkInputDataController SetValue float Unmatched type {GetType()}");
        }

        // debug
        public virtual string DisplayValue(BitArray bits)
        {
            throw new InvalidOperationException($"NetworkInputDataController DisplayValue Unmatched type {GetType()}");
        }
    }
}
