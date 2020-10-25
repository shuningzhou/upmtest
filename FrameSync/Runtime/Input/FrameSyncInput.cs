using SWNetwork.Core;
using System;
using System.Collections.Generic;
using Parallel;

namespace SWNetwork.FrameSync
{
    public interface IFrameSyncInputProvider
    {
        SWBytes CurrentInputFrame { get; }
    }

    public class FrameSyncInput
    {
        //FRAME
        static IFrameSyncInputProvider _inputFrameProvider;
        public void SetInputProvider(IFrameSyncInputProvider provider)
        {
            _inputFrameProvider = provider;
        }

        public void InputJustCopied(SWBytes bytes)
        {
            foreach(KeyValuePair<byte, FrameSyncPlayer> pair in _playerDictionary)
            {
                pair.Value.InputJustCopied(bytes);
            }
        }

        public void InputDeltaJustApplied(SWBytes bytes)
        {
            foreach (KeyValuePair<byte, FrameSyncPlayer> pair in _playerDictionary)
            {
                pair.Value.InputDeltaJustApplied(bytes);
            }
        }

        public void ExportInput(SWBytes bytes, bool offline)
        {
            byte playerCount = 0;
            foreach (KeyValuePair<byte, FrameSyncPlayer> pair in _playerDictionary)
            {
                FrameSyncPlayer player = pair.Value;
                if(player.Type == FrameSyncPlayerType.Local || player.Type == FrameSyncPlayerType.LocalBot)
                {
                    playerCount++;
                    pair.Value.ExportInput(bytes);
                }
            }

            if(!offline)
            {   
                //offline mode directyly written to input delta buffer
                //add playerCount so server 
                bytes.PushFront(playerCount);
            } 
        }

        //PLAYER
        Dictionary<byte, FrameSyncPlayer> _playerDictionary = new Dictionary<byte, FrameSyncPlayer>();
        public FrameSyncPlayer GetPlayer(byte playerID)
        {
            if(!_playerDictionary.ContainsKey(playerID))
            {
                return null;
            }

            return _playerDictionary[playerID];
        }

        internal FrameSyncPlayer CreatePlayer(byte playerID)
        {
            if (_inputConfig == null)
            {
                throw new InvalidOperationException("SWFrameSyncInputConfig not set");
            }

            if (_playerDictionary.ContainsKey(playerID))
            {
                return _playerDictionary[playerID];
            }

            FrameSyncPlayer player = new FrameSyncPlayer(playerID, _inputConfig);
            _playerDictionary[playerID] = player;
            return player;
        }

        //CONFIG
        // should never change once SetNetworkInputConfig() is called
        FrameSyncInputConfig _inputConfig = null;

        public byte Size
        {
            get
            {
                return _inputConfig.ByteSize;
            }
        }

        public FrameSyncInputConfig inputConfig
        {
            get
            {
                return _inputConfig;
            }
        }

        public FrameSyncInput(FrameSyncInputConfig inputConfig)
        {
            if (_inputConfig != null)
            {
                throw new InvalidOperationException("SWFrameSyncInputConfig cannot be changed");
            }

            _inputConfig = new FrameSyncInputConfig(inputConfig);
            int index = 0;
            foreach (FrameSyncInputSetting inputSetting in _inputConfig.InputSettings)
            {
                _inputIndexDictionary[inputSetting.Name] = index;
                index++;
            }
        }

        //INPUT
        Dictionary<String, int> _inputIndexDictionary = new Dictionary<string, int>();

        public Fix64 GetFloatForPlayer(string name, FrameSyncPlayer player)
        {
            return player.GetFloat(_inputIndexDictionary[name]);
        }

        public void SetFloatForPlayer(string name, Fix64 value, FrameSyncPlayer player)
        {
            player.SetFloat(_inputIndexDictionary[name], value);
        }

        public bool GetTriggerForPlayer(string name, FrameSyncPlayer player)
        {
            return player.GetTrigger(_inputIndexDictionary[name]);
        }

        public void SetTriggerForPlayer(string name, bool value, FrameSyncPlayer player)
        {
            player.SetTrigger(_inputIndexDictionary[name], value);
        }

        public int GetIntForPlayer(string name, FrameSyncPlayer player)
        {
            return player.GetInt(_inputIndexDictionary[name]);
        }

        public void SetIntForPlayer(string name, int value, FrameSyncPlayer player)
        {
            player.SetInt(_inputIndexDictionary[name], value);
        }

        //debug
        public IEnumerable<FrameSyncPlayer> _Players()
        {
            foreach(var pair in _playerDictionary)
            {
                yield return pair.Value;
            }
        }

        public int GetIndexForInput(string name)
        {
            return _inputIndexDictionary[name];
        }
    }
}
