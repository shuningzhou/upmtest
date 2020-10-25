//using SWNetwork.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SWNetwork.FrameSync
//{
//    internal class FloatInputDataController : SWFrameSyncInputDataController
//    {
//        float _userUpdatedValue;

//        internal FloatInputDataController(byte bitOffset) : base(bitOffset)
//        {

//        }

//        public override void Export(SWBytes buffer)
//        {
//            buffer.Push(_userUpdatedValue);
//        }

//        public override float GetFloatValue(SWBytes bytes, byte playerOffset)
//        {
//            return bytes.ReadFloat(playerOffset + _offset);
//        }

//        public override void SetValue(float value)
//        {
//            _userUpdatedValue = value;
//        }

//        //Debug
//        public override string DisplayValue(SWBytes bytes, byte playerOffset)
//        {
//            float v = bytes.ReadFloat(playerOffset + _offset);
//            return v.ToString();
//        }
//    }
//}
