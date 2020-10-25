using SWNetwork.Core;
using System.Collections;
using Parallel;

namespace SWNetwork.FrameSync
{
    internal class CompressedFloatInputDataController : FrameSyncInputDataController
    {
        Fix64 _userUpdatedValue;
        Fix64 _min;
        Fix64 _max;
        Fix64 _precision;
        byte _size;
        int _nvalues;
        Fix64 _defaultValue = Fix64.zero;

        internal CompressedFloatInputDataController(byte bitOffset, Fix64 min, Fix64 max, Fix64 precision, byte size, Fix64 defaultValue) : base(bitOffset)
        {
            _min = min;
            _max = max;
            _precision = precision;
            _nvalues = (int)((_max - _min) / precision) + 1;
            _size = size;
            _defaultValue = defaultValue;
            if (_defaultValue < _min || _defaultValue > _max)
            {
                SWConsole.Error($"Default value({_defaultValue}) should be between {_min} and {_max}. Using {_min} as the default value.");
                _defaultValue = _min;
            }
        }

        public override void Export(BitArray bitArray)
        {
            int delta;

            //min = -1.0
            //max = 1.0
            //precision = 0.5
            //default = 0.0

            //-1.0, -0.5, 0, 0.5, 1.0
            // 0,  1, 2, 3, 4
            //if user value less than default value
            //the delta = (user value / precision) + number of values
            //for example, for user value -1.0, delta = (-1.0)/(0.5) + 5 = -2 + 5 =3
            if (_userUpdatedValue < _defaultValue)
            {
                Fix64 v = _userUpdatedValue / _precision;
                int intV = (int)v;
                delta = intV + _nvalues;
            }
            else
            {
                delta = (int)((_userUpdatedValue - _defaultValue) / _precision);
            }

            for (int i = 0; i < _size; i++)
            {
                bitArray[i + _bitOffset] = ((delta >> i) & 1) == 1;
            }
        }

        public override Fix64 GetFloatValue(BitArray bitArray)
        {
            int result = 0;

            for (int i = 0; i < _size; i++)
            {
                int index = _bitOffset + i;
                bool bit = bitArray[index];
                if (bit)
                {
                    result |= 1 << i;
                }

            }

            //min = -1.0
            //max = 1.0
            //precision = 0.5
            //default = 0.0

            //-1.0, -0.5, 0, 0.5, 1.0
            // 0,  1, 2, 3, 4
            //if result is greater than the max value
            //the actual value = (result - number of values) * precision
            //for example, for result 3, actual value = (3 - 5) * 0.5 = -2 * 0.5 = -1.0
            Fix64 floatResult = (Fix64)result * _precision;
            //is result valid?
            if (result > _nvalues)
            {
                SWConsole.Error($"invalid result({result}): result={result} min={_min} max={_max} default={_defaultValue}");
                floatResult = _defaultValue;
            }
            else if (floatResult > _max)
            {
                floatResult = (Fix64)(result - _nvalues) * _precision;
            }

            return floatResult;
        }

        public override void SetValue(Fix64 value)
        {
            _userUpdatedValue = value;

            if (_userUpdatedValue < _min)
            {
                _userUpdatedValue = _min;
            }
            else if (_userUpdatedValue > _max)
            {
                _userUpdatedValue = _max;
            }
        }

        //Debug
        public override string DisplayValue(BitArray bitArray)
        {
            Fix64 v = GetFloatValue(bitArray);
            return v.ToString();
        }
    }
}
