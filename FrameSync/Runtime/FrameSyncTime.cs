using Parallel;
using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public static class FrameSyncTime
    {
        //user facting deltaTime
        public static Fix64 fixedDeltaTime { get; private set; }

        //internal deltaTime
        //this is the deltaTime we want to use for the FrameSyncEngine
        internal static float internalFixedDeltaTime { get; private set; }
        internal static float internalInputSampleInterval { get; private set; }

        static float _adjustInterval;
        static float _adjustTimer;
        static bool _prediction;
        static int _avgCount;
        static float _avgA;
        static float _avgB;

        static float _avgServerPlayerFrameCount;
        static float _avgLocalServerFrameCount;
        static float _avgPredictionError;

        static float _minDeltaTime;
        static float _maxDeltaTime;

        internal static void Initialize(Fix64 deltaTimeInSeconds, bool prediction, float adjustInterval, int avgCount)
        {
            fixedDeltaTime = deltaTimeInSeconds;
            internalFixedDeltaTime = (float)deltaTimeInSeconds;
            internalInputSampleInterval = (float)deltaTimeInSeconds;

            _prediction = prediction;
            _adjustInterval = adjustInterval;
            _adjustTimer = 0;
            _avgCount = avgCount;

            _maxDeltaTime = (float)deltaTimeInSeconds * (1 + FrameSyncConstant.DYNAMIC_ADJUST_MAX);
            _minDeltaTime = (float)deltaTimeInSeconds * (1 - FrameSyncConstant.DYNAMIC_ADJUST_MAX);

            if (_avgCount < 1)
            {
                _avgA = 0;
                _avgB = 1;
            }
            else
            {
                _avgA = ((float)_avgCount - 1) / (float)_avgCount;
                _avgB = 1 / (float)_avgCount;
            }
        }

        public static bool Adjust(int serverPlayerFrameCount, int localServerFrameCount, float deltaTime)
        {
            _adjustTimer += deltaTime;

            if(_adjustTimer > _adjustInterval)
            {
                _adjustTimer = 0;
                //SWConsole.Warn($"======================Adjust=======================");
                //SWConsole.Warn($"Adjust serverPlayerFrameCount={serverPlayerFrameCount} localServerFrameCount={localServerFrameCount}");
                UpdateLocalServerFrameCount(localServerFrameCount);
                UpdateServerPlayerFrameCount(serverPlayerFrameCount);
                //SWConsole.Warn($"Adjust AVG _avgServerPlayerFrameCount={_avgServerPlayerFrameCount} _avgLocalServerFrameCount={_avgLocalServerFrameCount}");
                DoAdjustment();

                return true;
            }

            return false;
        }

        public static bool Adjust(int predictionError, float deltaTime)
        {
            _adjustTimer += deltaTime;

            if (_adjustTimer > _adjustInterval)
            {
                _adjustTimer = 0;
                SWConsole.Warn($"======================Adjust=======================");
                //SWConsole.Warn($"Adjust serverPlayerFrameCount={serverPlayerFrameCount} localServerFrameCount={localServerFrameCount}");
                UpdatePredictionError(predictionError);
                //SWConsole.Warn($"Adjust AVG _avgServerPlayerFrameCount={_avgServerPlayerFrameCount} _avgLocalServerFrameCount={_avgLocalServerFrameCount}");
                DoAdjustmentForPrediction();

                return true;
            }

            return false;
        }

        public static void UpdateServerPlayerFrameCount(int serverPlayerFrameCount)
        {
            if(_avgServerPlayerFrameCount == 0)
            {
                _avgServerPlayerFrameCount = serverPlayerFrameCount;
            }
            _avgServerPlayerFrameCount = _avgServerPlayerFrameCount * _avgA + (float)serverPlayerFrameCount * _avgB;
        }

        public static void UpdateLocalServerFrameCount(int localServerFrameCount)
        {
            if (_avgLocalServerFrameCount == 0)
            {
                _avgLocalServerFrameCount = localServerFrameCount;
            }

            _avgLocalServerFrameCount = _avgLocalServerFrameCount * _avgA + (float)localServerFrameCount * _avgB;
        }

        public static void UpdatePredictionError(int predictionError)
        {
            if (_avgPredictionError == 0)
            {
                _avgPredictionError = predictionError;
            }

            _avgPredictionError = _avgPredictionError * _avgA + (float)predictionError * _avgB;
        }

        static void DoAdjustmentForPrediction()
        {
            //if client prediction is enabled, input is sampled in fixed updated
            //so we only adjust fixed update delta time

            //error = actual server frame number - predicted server frame number
            if (_avgPredictionError > 1)
            {
                //predicted is less than actual
                //local should run faster so local can predicter a larger frame numbers
                internalFixedDeltaTime = internalFixedDeltaTime * FrameSyncConstant.DYNAMIC_ADJUST_STEP;
                if (internalFixedDeltaTime < _minDeltaTime)
                {
                    internalFixedDeltaTime = _minDeltaTime;
                }

                SWConsole.Warn($"Adjust FASTER internalFixedDeltaTime={internalFixedDeltaTime}");
            }
            else if (_avgPredictionError < -1)
            {
                //predicted is greater than actual
                //local should run slower so local can predict smaller frame numbers
                internalFixedDeltaTime = internalFixedDeltaTime / FrameSyncConstant.DYNAMIC_ADJUST_STEP;
                if (internalFixedDeltaTime > _maxDeltaTime)
                {
                    internalFixedDeltaTime = _maxDeltaTime;
                }

                SWConsole.Warn($"Adjust SLOWER internalFixedDeltaTime={internalFixedDeltaTime}");
            }
        }

        static void DoAdjustment()
        {
            if(_avgServerPlayerFrameCount > FrameSyncConstant.EXPECTED_SERVER_PLAYER_FRAME_COUNT_MAX)
            {
                //input sampling is running faster than server, server queued more player frames than expected
                //make input sample interval longer so less player frames are generated
                internalInputSampleInterval = internalInputSampleInterval / FrameSyncConstant.DYNAMIC_ADJUST_STEP;
                if(internalInputSampleInterval > _maxDeltaTime)
                {
                    internalInputSampleInterval = _maxDeltaTime;
                }
                //SWConsole.Warn($"DoAdjustment Input SLOWER internalInputSampleInterval={internalInputSampleInterval}");
            }
            else if(_avgServerPlayerFrameCount < FrameSyncConstant.EXPECTED_SERVER_PLAYER_FRAME_COUNT_MIN)
            {
                //maybe local is running to slow
                //make input sample interval slightly shorter
                internalInputSampleInterval = internalInputSampleInterval * FrameSyncConstant.DYNAMIC_ADJUST_SMALL_STEP;
                if (internalInputSampleInterval < _minDeltaTime)
                {
                    internalInputSampleInterval = _minDeltaTime;
                }
                //SWConsole.Warn($"DoAdjustment Input FASTER internalInputSampleInterval={internalInputSampleInterval}");
            }

            if (_avgLocalServerFrameCount > FrameSyncConstant.EXPECTED_LOCAL_SERVER_FRAME_COUNT_MAX)
            {
                //local is running slower than server, local queued more server frames than expected
                //make local run faster so room frames are consumed faster
                internalFixedDeltaTime = internalFixedDeltaTime * FrameSyncConstant.DYNAMIC_ADJUST_STEP;
                if (internalFixedDeltaTime < _minDeltaTime)
                {
                    internalFixedDeltaTime = _minDeltaTime;
                }
                //SWConsole.Warn($"DoAdjustmen Game FASTER internalFixedDeltaTime={internalFixedDeltaTime}");
            }
            else if(_avgLocalServerFrameCount < FrameSyncConstant.EXPECTED_LOCAL_SERVER_FRAME_COUNT_MIN)
            {
                //local is running faster than server, local queued less server frames than expected
                //make local run slower so room frames are consumed slower
                internalFixedDeltaTime = internalFixedDeltaTime / FrameSyncConstant.DYNAMIC_ADJUST_STEP;
                if (internalFixedDeltaTime > _maxDeltaTime)
                {
                    internalFixedDeltaTime = _maxDeltaTime;
                }
                //SWConsole.Warn($"DoAdjustmen Game SLOWER internalFixedDeltaTime={internalFixedDeltaTime}");
            }
        }
    }
}