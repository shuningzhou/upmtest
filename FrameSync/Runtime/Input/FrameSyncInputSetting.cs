using Parallel;
using SWNetwork.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWNetwork.FrameSync
{
    public struct FrameSyncInputSetting
    {
        public FrameSyncInputType _inputType;

        public String _name;

        public byte _bitSize;

        //int
        public int _minInt;
        public int _maxInt;
        public int _defaultValueInt;

        //float
        public Fix64 _minFloat;
        public Fix64 _maxFloat;
        public Fix64 _precisionFloat;
        public Fix64 _defaultValueFloat;

        public static FrameSyncInputSetting NullSetting
        {
            get
            {
                FrameSyncInputSetting setting = new FrameSyncInputSetting();
                setting._inputType = FrameSyncInputType.Null;
                setting._name = "NULL";
                setting._bitSize = 0;
                return setting;
            }
        }

        public static FrameSyncInputSetting BoolInput(string name)
        {
            FrameSyncInputSetting setting = new FrameSyncInputSetting();
            setting._inputType = FrameSyncInputType.Bool;
            setting._name = name;
            setting._bitSize = 1;
            return setting;
        }

        public static FrameSyncInputSetting TriggerInput(string name)
        {
            FrameSyncInputSetting setting = new FrameSyncInputSetting();
            setting._inputType = FrameSyncInputType.Trigger;
            setting._name = name;
            setting._bitSize = 1;
            return setting;
        }

        static byte CompressedIntSize(int min, int max)
        {
            int delta = max - min;
            int nvalues = delta + 1;
            //Debug.Log("nvalues: " + nvalues);
            int length = (int)(Math.Ceiling(Math.Log(nvalues, 2)));
            //Debug.Log("bit size: " + length);
            return (byte)length;
        }

        static byte CompressedFloatSize(Fix64 min, Fix64 max, Fix64 precision)
        {
            Fix64 delta = max - min;
            int nvalues = (int)(delta / precision) + 1;
            //Debug.Log("nvalues: " + nvalues);
            int length = (int)(Math.Ceiling(Math.Log(nvalues, 2)));
            //Debug.Log("bit size: " + length);
            return (byte)length;
        }

        public static FrameSyncInputSetting CompressedIntInput(string name, int min, int max, int defaultValue)
        {
            if(min >= max)
            {
                SWConsole.Error("");
                return NullSetting;
            }

            FrameSyncInputSetting setting = new FrameSyncInputSetting();
            setting._inputType = FrameSyncInputType.CompressedInt;
            setting._name = name;
            setting._bitSize = CompressedIntSize(min, max);
            setting._minInt = min;
            setting._maxInt = max;
            setting._defaultValueInt = defaultValue;

            return setting;
        }

        public static FrameSyncInputSetting CompressedFloatInput(string name, Fix64 min, Fix64 max, Fix64 precision, Fix64 defaultValue)
        {
            if (min >= max)
            {
                SWConsole.Error("");
                return NullSetting;
            }

            FrameSyncInputSetting setting = new FrameSyncInputSetting();
            setting._inputType = FrameSyncInputType.CompressedFloat;
            setting._name = name;
            setting._bitSize = CompressedFloatSize(min, max, precision);
            setting._minFloat = min;
            setting._maxFloat = max;
            setting._precisionFloat = precision;
            setting._defaultValueFloat = defaultValue;

            return setting;
        }

        public String Name
        {
            get
            {
                return _name;
            }
        }

        public byte BitSize
        {
            get
            {
                return _bitSize;
            }
        }

        internal FrameSyncInputDataController MakeInputDataController(byte bitOffset)
        {
            switch (_inputType)
            {
                case FrameSyncInputType.CompressedInt:
                    return new CompressedIntInputDataController(bitOffset, _minInt, _maxInt, _bitSize, _defaultValueInt);
                case FrameSyncInputType.CompressedFloat:
                    return new CompressedFloatInputDataController(bitOffset, _minFloat, _maxFloat, _precisionFloat, _bitSize, _defaultValueFloat);
                case FrameSyncInputType.Trigger:
                    return new TriggerInputDataController(bitOffset);
                default:
                    throw new InvalidOperationException($"Input settings invalid input type {_inputType}");
            }
        }

        public static bool operator == (FrameSyncInputSetting a, FrameSyncInputSetting b) 
        { 
            
            if(a._name == b._name &&
                a._inputType == b._inputType &&
                a._bitSize == b._bitSize)
            {

                return true;
            }

            return false;
        }

        public static bool operator !=(FrameSyncInputSetting a, FrameSyncInputSetting b)
        {

            if (a._name != b._name ||
                a._inputType != b._inputType ||
                a._bitSize != b._bitSize)
            {

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode() + _bitSize.GetHashCode() + _inputType.GetHashCode() + _minInt.GetHashCode() + _maxInt.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FrameSyncInputSetting))
                return false;
            return ((FrameSyncInputSetting)obj) == this;
        }
    }
}
