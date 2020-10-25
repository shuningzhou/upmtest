using Parallel;
using SWNetwork.Core;
using System;
using System.Collections.Generic;

namespace SWNetwork.FrameSync
{
    public static class FrameSyncRandom
    {
        //random
        static UInt32 _randomValue;

        internal static void _internal_seed(UInt32 seed)
        {
            //seems that if seed is too small, the distribution of the random numbers are not good.
            //add 100 so the seed is not too small
            _randomValue = seed + 100;
            // discard first output as it's essentially just the seed
            Next();
        }

        // https://en.wikipedia.org/wiki/Xorshift
        // not thread safe
        static UInt32 Next()
        {
            UInt32 result = _randomValue;
            result = result ^ _randomValue << 13;
            result = result ^ _randomValue >> 17;
            result = result ^ _randomValue << 5;
            _randomValue = result;
            return result;
        }

        static UInt32 Next(UInt32 max)
        {
            UInt32 result = Next();
            result = (UInt32)((UInt64)result * (UInt64)max >> 32);

            return result;
        }

        public static int Range(int min, int max)
        {
            if(min > max)
            {
                return 0;
            }

            if(min == max)
            {
                return min;
            }

            UInt32 delta = (UInt32)(max - min);
            UInt32 result = Next(delta);
            int intResult = min + (int)result;
            return intResult;
        }

        public static Fix64 Range(Fix64 min, Fix64 max)
        {
            if(min > max)
            {
                return Fix64.zero;
            }

            if(min == max)
            {
                return min;
            }

            Fix64 precision = Fix64.FromDivision(1, 100);
            UInt32 delta = (UInt32)((int)((max - min) / precision) + 1);
            UInt32 result = Next(delta);

            Fix64 floatValue = min + precision * (Fix64)result;
            return floatValue;
        }

        public static void RandromTest()
        {
            Dictionary<UInt32, int> dict = new Dictionary<uint, int>();
            SortedList<UInt32, int> distribution = new SortedList<UInt32, int>();
            _internal_seed(1);
            int count = 1000;
            uint groupSize = 5;
            for (int i = 0; i < count; i++)
            {
                UInt32 result = Next(100);
                SWConsole.Info($"PRandom = {result}");
                if (dict.ContainsKey(result))
                {
                    dict[result]++;
                }
                else
                {
                    dict[result] = 1;
                }

                UInt32 group = result / groupSize;

                if (distribution.ContainsKey(group))
                {
                    distribution[group]++;
                }
                else
                {
                    distribution[group] = 1;
                }
            }

            foreach (var pair in distribution)
            {
                SWConsole.Info($"{pair.Key} = {pair.Value}");
            }
        }
    }
}