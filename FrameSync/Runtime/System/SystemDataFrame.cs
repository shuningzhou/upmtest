using SWNetwork.Core;
using SWNetwork.Core.DataStructure;
using System;

namespace SWNetwork.FrameSync
{
    public class SWSystemDataFrame : IPersistentArrayData
    {
        public SWBytes bytes;

        internal int FrameNumber;

        public SWSystemDataFrame()
        {
            bytes = new SWBytes(FrameSyncConstant.DATA_FRAME_SIZE);
            FrameNumber = 0;
        }

        internal SWSystemDataFrame(int frameNumber)
        {
            bytes = new SWBytes(FrameSyncConstant.DATA_FRAME_SIZE);
            FrameNumber = frameNumber;
        }

        internal void ResetBytes()
        {
            bytes.Reset();
        }

        public void Export(SWBytes buffer)
        {
            buffer.PushFront((UInt16)bytes.DataLength);
            buffer.PushAll(bytes);
        }

        public void Import(SWBytes buffer)
        {
            UInt16 dataLength = buffer.PopUInt16();
            buffer.PopByteBuffer(bytes, 0, (int)dataLength);
        }
    }
}
