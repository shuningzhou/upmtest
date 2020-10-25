using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public class SaveReplayoperation : Operation
    {
        byte[] _data;
        string _file;

        public SaveReplayoperation(byte[] data, string fileName)
        {
            _data = data;
            _file = FrameSyncConstant.DEFAULT_DIRECTORY + fileName;
        }

        public override void Execute()
        {
            SWLocalStorage.SaveToFile(_file, _data, false);
        }
    }
}
