using UnityEngine;
using SWNetwork.Core;
using System.Collections.Generic;
using SWNetwork.Core.DataStructure;

namespace SWNetwork.FrameSync
{
    public class FrameSyncClient : IFrameSyncPlayerDataProvider
    {
        public delegate void ClientReady(bool ready);
        public static ClientReady OnClientReadyEvent;

        public static FrameSyncClient Instance = null;
        private SWGameServerClient _client;

        static string _debugName = "SWFrameSyncClient";

        public static void Init(string playerUID)
        {
            if (Instance != null)
            {
                return;
            }

            Instance = new FrameSyncClient(playerUID);
            UnityThread.initUnityThread();

            if (UnityEngine.Application.isPlaying)
            {
                GameObject obj = new GameObject("_SWFrameSyncClientBehaviour");
                //obj.hideFlags = HideFlags.HideAndDontSave;
                SWFrameSyncClientBehaviour cb = obj.AddComponent<SWFrameSyncClientBehaviour>();
                cb.SetClient(Instance);
            }
        }

        FrameSyncClient(string playerUID)
        {
            _client = new SWGameServerClient("127.0.0.1", playerUID, "roomKey");

            //todo: this should be create in the match making phase 
            _playerDataBiMap.Clear();
            _playerDataBiMap.Add("1", 1);
            _playerDataBiMap.Add("2", 2);
        }

        public IFrameSyncIO frameSyncIO
        {
            get
            {
                if (Instance != null && Instance._client != null)
                {
                    return Instance._client;
                }
                return null;
            }
        }

        public IFrameSyncPlayerDataProvider playerDataProvider
        {
            get
            {
                if (Instance != null && Instance._client != null)
                {
                    return Instance;
                }
                return null;
            }
        }

        public static int ServerPing
        {
            get
            {
                if(Instance != null && Instance._client != null)
                {
                    return Instance._client.Ping;
                }
                return 0;
            }
        }

        public static void Connect()
        {
            if (Instance != null)
            {
                Instance._client.OnGameServerConnectionReadyEvent += OnGameServerConnectionReady;

                Instance._client.Connect();
            }
        }

        public static void Disconnect()
        {
            if (Instance != null)
            {
                Instance._client.OnGameServerConnectionReadyEvent -= OnGameServerConnectionReady;
                Instance._client.Stop();
            }
        }

        /* Private */
        private static void OnGameServerConnectionReady(bool ready)
        {
            UnityThread.executeInUpdate(() =>
            {
                OnClientReadyEvent(ready);
            });
        }

        internal void OnUnityApplicationQuit()
        {
            SWConsole.Info($"{_debugName} Application ending after " + Time.time + " seconds");
            Disconnect();
        }

        //playerDataProvider
        BiMap<string, byte> _playerDataBiMap= new BiMap<string, byte>();

        public byte localPlayerRoomID
        {
            get
            {
                return _client.PlayerRoomID;
            }
        }

        public IEnumerable<byte> playerRoomIDs
        {
            get
            {
                foreach(var item in _playerDataBiMap.values)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<string> playerUIDs
        {
            get
            {
                foreach (var item in _playerDataBiMap.keys)
                {
                    yield return item;
                }
            }
        }

        public string GetPlayerUID(byte playerRoomID)
        {
            return _playerDataBiMap.GetKey(playerRoomID);
        }

        public byte GetPlayerRoomID(string playerUID)
        {
            return _playerDataBiMap.GetValue(playerUID);
        }

        public T GetUserData<T>()
        {
            //todo
            //should be similar to room customData
            return default(T);
        }
    }
}
