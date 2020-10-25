using System;
using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public class FrameSyncGame
    {
        internal FrameSyncGameState _gameState;
        public FrameSyncGameState gameState
        {
            get
            {
                return _gameState;
            }

            internal set
            {
                _gameState = value;
                if(_gameState == FrameSyncGameState.Stopped)
                {
                    _inputPlayerCreatedHandler = null;
                    _offlineGamePlayerID = 1;
                }
            }
        }

        public int frameNumber
        {
            get;
            internal set;
        }

        internal FrameSyncGameType _type = FrameSyncGameType.Offline;
        public FrameSyncGameType type
        {
            get
            {
                return _type;
            }
            set
            {
                if(_gameState != FrameSyncGameState.Default)
                {
                    SWConsole.Error("SWFrameSyncGame: not allow to change the game type after starting the game");
                    return;
                }

                _type = value;
            }
        }

        object _userData;
        public object userData
        {
            get
            {
                return _userData;
            }

            set
            {
                _userData = value;
            }
        }

        IFrameSyncPlayerDataProvider _playerDataProvider;
        public void SetPlayerDataProvider(IFrameSyncPlayerDataProvider provider)
        {
            _playerDataProvider = provider;
        }

        public void CreateOnlinePlayers()
        {
            if(_playerDataProvider != null)
            {
                foreach (byte playerRoomID in _playerDataProvider.playerRoomIDs)
                {
                    byte localPlayerRoomID = _playerDataProvider.localPlayerRoomID;

                    CreateOnlineGamePlayer(playerRoomID, localPlayerRoomID);
                }
            }
        }

        public void CreateGameUserData<T>()
        {
            if (_playerDataProvider != null)
            {
                //_userData = _playerDataProvider.GetUserData<T>();

                ////todo
                ////_userData should be create in the matchmaking stage
                ///
                //GameSettings gameSettings = new GameSettings();
                //gameSettings.player1ID = 1;
                //gameSettings.player2ID = 2;

                //_userData = gameSettings;
            }
        }

        string _replayFileName;
        public string replayFileName
        {
            get
            {
                return _replayFileName;
            }

            set
            {
                if (_gameState != FrameSyncGameState.Default)
                {
                    SWConsole.Error("SWFrameSyncGame: not allow to change SaveInputFile after starting the game");
                    return;
                }

                _replayFileName = value;
            }
        }

        internal FrameSyncInput _input;
        public FrameSyncGame(FrameSyncInput input)
        {
            _input = input;
        }

        byte _offlineGamePlayerID = 1;
        public FrameSyncPlayer CreateOfflineGamePlayer()
        {
            if(_gameState != FrameSyncGameState.Default)
            {
                SWConsole.Error("SWFrameSyncGame: not allow to add players after starting the game");
                return null;
            }

            FrameSyncPlayer player = _input.CreatePlayer(_offlineGamePlayerID);
            _offlineGamePlayerID++;

            OnNewPlayerCreated(player);

            return player;
        }

        public FrameSyncPlayer GetPlayer(byte playerID)
        {
            return _input.GetPlayer(playerID);
        }

        public FrameSyncPlayer localPlayer
        {
            get
            {
                byte localPlayerRoomID = _playerDataProvider.localPlayerRoomID;
                return GetPlayer(localPlayerRoomID);
            }
        }

        internal FrameSyncPlayer CreateOnlineGamePlayer(byte playerID, byte localPlayerID)
        {
            FrameSyncPlayer player = _input.CreatePlayer(playerID);
            if(playerID == localPlayerID)
            {
                player.Type = FrameSyncPlayerType.Local;
            }
            else{
                player.Type = FrameSyncPlayerType.Remote;
            }

            OnNewPlayerCreated(player);
            return player;
        }

        Action<FrameSyncPlayer> _inputPlayerCreatedHandler;
        public Action<FrameSyncPlayer> inputPlayerCreatedHandler
        {
            set
            {
                _inputPlayerCreatedHandler = value;
            }
        }

        internal void OnNewPlayerCreated(FrameSyncPlayer player)
        {
            if(_inputPlayerCreatedHandler != null)
            {
                _inputPlayerCreatedHandler.Invoke(player);
            }
        }

        bool _clientSidePrediction = false;
        public bool clientSidePrediction
        {
            get
            {
                return _clientSidePrediction;
            }
            set
            {
                _clientSidePrediction = value;
            }
        }

        float _stepInterval = FrameSyncConstant.FRAME_SYNC_FIXED_UPDATE_TIME;
        internal float StepInterval
        {
            get
            {
                return _stepInterval;
            }
        }
    }
}
