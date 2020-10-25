using SWNetwork.Core;
using SWNetwork.Core.DataStructure;

namespace SWNetwork.FrameSync
{
    public class InputFrame : IPersistentArrayData
    {
        public SWBytes bytes;

        internal int FrameNumber;

        public InputFrame()
        {
            bytes = new SWBytes(FrameSyncConstant.INPUT_FRAME_SIZE);
            FrameNumber = 0;
        }

        internal InputFrame(int frameNumber)
        {
            bytes = new SWBytes(FrameSyncConstant.INPUT_FRAME_SIZE);
            FrameNumber = frameNumber;
        }

        internal void ResetBytes()
        {
            bytes.Reset();
        }

        public void Export(SWBytes buffer)
        {
            buffer.PushFront((byte)bytes.DataLength);
            buffer.PushAll(bytes);
        }

        public void Import(SWBytes buffer)
        {
            byte dataLength = buffer.PopByte();
            buffer.PopByteBuffer(bytes, 0, (int)dataLength);
        }
    }
}
