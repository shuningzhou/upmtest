using Parallel;
using SWNetwork.Core;
using System.Collections;
using System.Collections.Generic;

namespace SWNetwork.FrameSync
{
    public class FrameSyncPlayer
    {
        internal byte _inputOffset;
        internal byte _byteSize;
        BitArray _bitArray;
        BitArray _exportBitArray;
        byte[] _byteArray;
        byte[] _exportByteArray;

        internal byte InputOffset
        {
            get
            {
                return _inputOffset;
            }
        }

        internal FrameSyncPlayerType _type;
        internal FrameSyncPlayerType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        internal byte _playerID;
        public byte PlayerID
        {
            get
            {
                return _playerID;
            }
        }

        public FrameSyncPlayer(byte playerID, FrameSyncInputConfig config)
        {
            _playerID = playerID;

            byte bitOffset = 0;
            foreach (FrameSyncInputSetting inputSetting in config.InputSettings)
            {
                _inputDataControllers.Add(inputSetting.MakeInputDataController(bitOffset));
                bitOffset = (byte)(bitOffset + inputSetting.BitSize);
            }

            _byteSize = config.ByteSize;
            _byteArray = new byte[_byteSize];
            _exportByteArray = new byte[_byteSize];
            _exportBitArray = new BitArray(_exportByteArray);
            _inputOffset = (byte)((_playerID - 1) * _byteSize);
        }

        //INPUT
        List<FrameSyncInputDataController> _inputDataControllers = new List<FrameSyncInputDataController>();

        //for trigger to reset value
        internal void InputJustCopied(SWBytes nextFrameBytes)
        {
            nextFrameBytes.ReadByteArray(_inputOffset, _byteArray);
            _bitArray = new BitArray(_byteArray);

            foreach (FrameSyncInputDataController controller in _inputDataControllers)
            {
                controller.InputJustCopied(_bitArray);
            }

            _bitArray.CopyTo(_byteArray, 0);

            nextFrameBytes.WriteByteArray(_inputOffset, _byteArray);
        }

        //for preparing bitarray
        internal void InputDeltaJustApplied(SWBytes nextFrameBytes)
        {
            nextFrameBytes.ReadByteArray(_inputOffset, _byteArray);
            _bitArray = new BitArray(_byteArray);

        }

        internal Fix64 GetFloat(int index)
        {
            return _inputDataControllers[index].GetFloatValue(_bitArray);
        }

        internal void SetFloat(int index, Fix64 value)
        {
            _inputDataControllers[index].SetValue(value);
        }

        internal bool GetTrigger(int index)
        {
            return _inputDataControllers[index].GetBoolValue(_bitArray);
        }

        internal void SetTrigger(int index, bool value)
        {
            _inputDataControllers[index].SetValue(value);
        }

        internal int GetInt(int index)
        {

            return _inputDataControllers[index].GetIntValue(_bitArray);
        }

        internal void SetInt(int index, int value)
        {
            _inputDataControllers[index].SetValue(value);
        }

        //EXPORT INPUT
        internal void ExportInput(SWBytes data)
        {
            data.Push(PlayerID);

            foreach (FrameSyncInputDataController controller in _inputDataControllers)
            {
                controller.Export(_exportBitArray);
            }

            _exportBitArray.CopyTo(_exportByteArray, 0);
            data.Push(_exportByteArray);
        }

        //DEBUG
        Dictionary<string, string> _debugDict = new Dictionary<string, string>();
        internal Dictionary<string, string> ExportDictionary(FrameSyncInputConfig inputConfig, SWBytes bytes)
        {
            bytes.ReadByteArray(_inputOffset, _exportByteArray);
            _exportBitArray = new BitArray(_exportByteArray);

            int index = 0;
            foreach(FrameSyncInputSetting s in inputConfig.InputSettings)
            {
                string displayValue = _inputDataControllers[index].DisplayValue(_exportBitArray);
                _debugDict[s.Name] = displayValue;
                index++;
            }

            return _debugDict;
        }
    }
}
