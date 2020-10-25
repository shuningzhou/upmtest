using UnityEngine;
using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public class SWFrameSyncClientBehaviour : MonoBehaviour
    {
        private static SWFrameSyncClientBehaviour instance = null;
        FrameSyncClient _client;

        void Awake()
        {
            if (instance)
            {
                Destroy(this);
            }

            DontDestroyOnLoad(gameObject);
            instance = this;
            SWConsole.Info("SWClientBehaviour awake");
        }

        internal void SetClient(FrameSyncClient client)
        {
            _client = client;
        }

        private void LateUpdate()
        {
            //ProfileUtil.Step();
            //_client.OnUnityLateUpdate();
        }

        private void Update()
        {
            //ProfileUtil.Step();
        }

        private void FixedUpdate()
        {
            //ProfileUtil.Step();
            //_client.OnUnityFixedUpdate();
        }

        private void OnApplicationQuit()
        {
            _client.OnUnityApplicationQuit();
            //SWClient.StopFrameSyncFixedUpdater();
        }
    }

}
