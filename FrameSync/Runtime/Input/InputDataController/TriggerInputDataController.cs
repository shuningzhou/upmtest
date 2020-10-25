using SWNetwork.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWNetwork.FrameSync
{
    class TriggerInputDataController : FrameSyncInputDataController
    {
        bool _userUpdatedValue;

        internal TriggerInputDataController(byte bitOffset) : base(bitOffset)
        {

        }

        public override void InputJustCopied(BitArray bitArray)
        {
            bitArray.Set(_bitOffset, false);
        }

        public override void Export(BitArray bitArray)
        {
            bitArray.Set(_bitOffset, _userUpdatedValue);
            _userUpdatedValue = false;
        }

        public override bool GetBoolValue(BitArray bitArray)
        {
            return bitArray.Get(_bitOffset);
        }

        public override void SetValue(bool value)
        {
            if(value == true)
            {
                _userUpdatedValue = true;
            }
        }

        //Debug
        public override string DisplayValue(BitArray bitArray)
        {
            bool v = bitArray.Get(_bitOffset);
            return v.ToString();
        }
    }
}