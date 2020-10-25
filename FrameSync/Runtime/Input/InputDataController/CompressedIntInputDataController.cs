using SWNetwork.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWNetwork.FrameSync
{
    internal class CompressedIntInputDataController : FrameSyncInputDataController
    {
        int _userUpdatedValue;
        int _min;
        int _max;
        byte _size;
        int _nvalues;
        int _defaultValue = 0;

        internal CompressedIntInputDataController(byte bitOffset, int min, int max, byte size, int defaultValue = 0) : base(bitOffset)
        {
            _min = min;
            _max = max;
            _nvalues = _max - _min + 1;
            _size = size;
            _defaultValue = defaultValue;
            if(_defaultValue < _min || _defaultValue > _max)
            {
                SWConsole.Error($"Default value({_defaultValue}) should be between {_min} and {_max}. Using {_min} as the default value.");
                _defaultValue = _min;
            }
        }

        public override void Export(BitArray bitArray)
        {
            int delta;

            //-2, -1, 0, 1, 2
            // 0,  1, 2, 3, 4
            //if user value less than default value
            //the delta = user value + number of values
            //for example, for user value -2, delta = -2 + 5 = 3
            if (_userUpdatedValue < _defaultValue)
            {
                delta = _userUpdatedValue + _nvalues;
            }
            else
            {
                delta = _userUpdatedValue - _defaultValue;
            }

            for (int i = 0; i < _size; i++)
            {
                bitArray[i + _bitOffset] = ((delta >> i) & 1) == 1;
            }
        }

        public override int GetIntValue(BitArray bitArray)
        {
            int result = 0;

            for(int i = 0; i < _size; i++)
            {
                int index = _bitOffset + i;
                bool bit = bitArray[index];
                if(bit)
                {
                    result |= 1 << i;
                }
                
            }

            //-2, -1, 0, 1, 2
            // 0,  1, 2, 3, 4
            //if result is greater than the max value
            //the actual value = result - number of values
            //for example, for result 3, actual value = 3 - 5 = -2

            //is result valid?
            if(result > _nvalues)
            {
                SWConsole.Error($"invalid result({result}): result={result} min={_min} max={_max} default={_defaultValue}");
                result = _defaultValue;
            }
            else if (result > _max)
            {
                result = result - _nvalues;
            }

            return result;
        }

        public override void SetValue(int value)
        {
            _userUpdatedValue = value;

            if (_userUpdatedValue < _min)
            {
                _userUpdatedValue = _min;
            }
            else if(_userUpdatedValue > _max)
            {
                _userUpdatedValue = _max;
            }
        }

        //Debug
        public override string DisplayValue(BitArray bitArray)
        {
            int v = GetIntValue(bitArray);
            return v.ToString();
        }
    }
}
