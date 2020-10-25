using SWNetwork.Core;
using System;
using SWNetwork.Core.DataStructure;

namespace SWNetwork.FrameSync
{
    public class InputFrameDelta : IPersistentArrayData
    {
        internal SWBytes bytes;

        internal int frameNumber;

        internal int predictedServerFrameNumber;

        internal byte version;

        internal bool isSealed;
        internal int resend;

        public static int DataSize = FrameSyncConstant.INPUT_FRAME_SIZE;

        public InputFrameDelta()
        {
            bytes = new SWBytes(FrameSyncConstant.INPUT_FRAME_SIZE);
            version = 0;
            frameNumber = 0;
            isSealed = false; 
        }

        internal InputFrameDelta(int frameNum)
        {
            bytes = new SWBytes(FrameSyncConstant.INPUT_FRAME_SIZE);
            version = 0;
            frameNumber = frameNum;
        }

        internal void ReadServerInputFrameDelta(SWBytes data, byte ver)
        {
            SWBytes.Copy(data, bytes);
            version = ver;
        }

        internal void ResetBytes()
        {
            bytes.Reset();
        }

        internal bool IsSameInput(InputFrameDelta other)
        {
            return Util.ByteArrayCompare(bytes.RawData(), other.bytes.RawData());
        }

        internal void Apply(FrameSyncInput input, InputFrame i1, InputFrame i2)
        {
            //copy i1 to i2
            SWBytes.CopyFull(i1.bytes, i2.bytes);

            //let input reset
            //important to reset triggers
            input.InputJustCopied(i2.bytes);

            //apply delta for each player
            byte inputSize = input.Size;

            //SWConsole.Crit($"ApplyDelta frameNumber={frameNumber} {bytes.FullString()}");

            while(bytes.DataLength > 0)
            {
                byte playerID = bytes.PopByte();
                FrameSyncPlayer player = input.GetPlayer(playerID);
                if(player == null)
                {
                    SWConsole.Error($"InputFrameDelta Apply: player not found {playerID}");
                }
                byte offset = player.InputOffset;
                SWBytes.Copy(bytes, i2.bytes, bytes.ReadIndex, offset, inputSize);
                bytes.SkipRead(inputSize);
            }

            //reset read index
            bytes.SetReadIndex(0);

            //prepare bitarray
            input.InputDeltaJustApplied(i2.bytes);
        }

        internal void ApplyPrediction(FrameSyncInput input, InputFrame inputFrame)
        {
            byte inputSize = input.Size;

            if (bytes.DataLength > 0)
            {
                byte playerCount = bytes.PopByte();

                for (int i = 0; i < playerCount; i++)
                {
                    byte playerID = bytes.PopByte();
                    FrameSyncPlayer player = input.GetPlayer(playerID);
                    byte offset = player.InputOffset;
                    SWBytes.Copy(bytes, inputFrame.bytes, bytes.ReadIndex, offset, inputSize);
                    bytes.SkipRead(inputSize);
                }

                //reset read index
                bytes.SetReadIndex(0);
            }
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
