using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWNetwork.FrameSync
{
    public class FrameSyncInputConfig
    {
        public FrameSyncInputSetting[] InputSettings;
        byte _byteSize;

        public byte ByteSize
        {
            get
            {
                return _byteSize;
            }
        }

        public FrameSyncInputConfig(FrameSyncInputSetting[] inputSettings)
        {
            //TODO: validate input size
            // should not be greater than 64 bytes

            InputSettings = inputSettings;

            byte bitSize = 0;
            foreach (FrameSyncInputSetting inputSetting in InputSettings)
            {
                bitSize = (byte)(bitSize + inputSetting.BitSize);
            }

            byte padding = (byte)(8 - bitSize % 8);

            if (padding == 8)
            {
                padding = 0;
            }

            bitSize = (byte)(bitSize + padding);

            _byteSize = (byte)(bitSize / 8);
        }

        public FrameSyncInputConfig(FrameSyncInputConfig other)
        {
            InputSettings = new FrameSyncInputSetting[other.InputSettings.Length];
            Array.Copy(other.InputSettings, InputSettings, other.InputSettings.Length);
            _byteSize = other._byteSize;
        }
    }
}
