using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWNetwork.FrameSync
{
    public enum FrameSyncInputType
    {
        Bool,
        Trigger,
        CompressedInt,
        CompressedFloat,
        Null
    }

    public enum FrameSyncGameState
    {
        Default,
        InitializingRoomFrame,
        WaitingForRoomFrame,
        WaitingForInitialSystemData,
        Running,
        Stopped
    }

    public enum FrameSyncPlayerType
    {
        Local,
        LocalBot,
        Remote,
        RemoteBot,
    }

    public enum FrameSyncGameType
    {
        Online,
        Offline
    }

    public enum FrameSyncUpdateType
    {
        Normal,
        Correction,
        Prediction,
        Restore
    }

    public enum FrameSyncBehaviourType
    {
        Dynamic,
        Static
    }

    public static class FrameSyncConstant
    {
        public const int DEFAULT_FRAMES_CHUNK_SIZE = 256;
        public const int INPUT_FRAME_SIZE = 28;
        public const int DATA_FRAME_SIZE = 1024 * 16;

        public const float FRAME_SYNC_FIXED_UPDATE_TIME = 0.02f;

        public const int FRAMESYNC_BEHAVIOUR_BUFFER_SIZE = 256;
        public const int FRAMESYNC_BEHAVIOUR_BUFFER_SIZE_LARGE = 1024 * 4;

        public const int SERVER_FRAME_INITIALIZATION_INTERVAL = 1000;

        public const int LOCAL_INPUT_FRAME_RESEND_COUNT = 3;

        //frameSyncTime
        public const float EXPECTED_SERVER_PLAYER_FRAME_COUNT_MAX = 1.1f;
        public const float EXPECTED_LOCAL_SERVER_FRAME_COUNT_MAX = 2.1f;
        public const float EXPECTED_SERVER_PLAYER_FRAME_COUNT_MIN = 0.1f;
        public const float EXPECTED_LOCAL_SERVER_FRAME_COUNT_MIN = 0.9f;
        public const float DYNAMIC_ADJUST_INTERVAL = 0.2f;
        public const float DYNAMIC_ADJUST_STEP = 0.95f;
        public const float DYNAMIC_ADJUST_SMALL_STEP = 0.99f;
        public const int DYNAMIC_AVERAGE_COUNT = 3;
        public const float DYNAMIC_ADJUST_MAX = 0.5f;

        //save replay
        public const int SAVE_REPLAY_BUFFER_SIZE_30_KB = 1024 * 30;
        public const int SAVE_REPLAY_BUFFER_SIZE_300_KB = 1024 * 300;
        public const int SAVE_REPLAY_BUFFER_SIZE_1_MB = 1024 * 1024;
        public const string DEFAULT_DIRECTORY = "C:/Users/victo/Desktop/test/persistenarray/";

        //prediction
        public const int PREDICTION_GLOBAL_DEBAY_FRAMES = 2;

        //debug server command
        public const byte DEBUG_SERVER_PLAYER_FRAME = 1;

    }
}
