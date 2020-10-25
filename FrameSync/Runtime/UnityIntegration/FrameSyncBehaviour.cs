using UnityEngine;
using System;
using SWNetwork.Core;
using System.Collections.Generic;

namespace SWNetwork.FrameSync
{
    public class FrameSyncBehaviour : MonoBehaviour
    {
        internal UInt16 _id;
        public UInt16 FrameSyncBehaviourID
        {
            get
            {
                return _id;
            }
        }

        internal UInt16 _prefabIndex;
        public UInt16 prefabID
        {
            get
            {
                return _prefabIndex;
            }
        }

        internal bool _isInitialized = false;

        public bool largeDataContainer = false;

        IFrameSyncData[] _frameSyncDatas;
        IFrameSyncUpdate[] _frameSyncUpdates;
        SWBytes _byteBuffer;

        public uint GenerateHash()
        {
            _byteBuffer.Reset();

            foreach (IFrameSyncData frameSyncData in _frameSyncDatas)
            {
                frameSyncData.Export(_byteBuffer);
            }

            return _byteBuffer.Crc32();
        }

        public void InvokeFrameSyncFixedUpdate(FrameSyncInput input, FrameSyncUpdateType frameSyncUpdateType)
        {
            foreach (IFrameSyncUpdate frameSyncUpdate in _frameSyncUpdates)
            {
                frameSyncUpdate.FrameSyncUpdate(input, frameSyncUpdateType);
            }
        }

        protected virtual void Awake()
        {
            initialize(GetComponents<IFrameSyncData>(), GetComponents<IFrameSyncUpdate>());
        }

        void initialize(IFrameSyncData[] frameSyncDatas, IFrameSyncUpdate[] frameSyncUpdates)
        {
            _frameSyncDatas = frameSyncDatas;
            _frameSyncUpdates = frameSyncUpdates;

            if(largeDataContainer)
            {
                _byteBuffer = new SWBytes(FrameSyncConstant.FRAMESYNC_BEHAVIOUR_BUFFER_SIZE_LARGE);
            }
            else
            {
                _byteBuffer = new SWBytes(FrameSyncConstant.FRAMESYNC_BEHAVIOUR_BUFFER_SIZE);
            }
        }

        internal void InvokeFrameSyncDataInitialize(FrameSyncGame game)
        {
            foreach (IFrameSyncData frameSyncData in _frameSyncDatas)
            {
                frameSyncData.FrameSyncDataInitialize(game);
            }
        }

        internal int ExportData(SWBytes buffer)
        {
            _byteBuffer.Reset();

            foreach (IFrameSyncData frameSyncData in _frameSyncDatas)
            {
                frameSyncData.Export(_byteBuffer);
            }

            int dataSize = 0;

            if(largeDataContainer)
            {
                UInt16 size = (UInt16)_byteBuffer.DataLength;

                buffer.Push(size);
                dataSize = 2 + size;
            }
            else
            {
                byte size = (byte)_byteBuffer.DataLength;

                buffer.Push(size);
                dataSize += 1 + size;
            }

            buffer.PushAll(_byteBuffer);

            return dataSize;
        }

        internal void ImportData(SWBytes buffer)
        {
            _byteBuffer.Reset();

            if (largeDataContainer)
            {
                UInt16 size = _byteBuffer.PopUInt16();
                buffer.PopByteBuffer(_byteBuffer, 0, size);
            }
            else
            {
                byte size = buffer.PopByte();
                buffer.PopByteBuffer(_byteBuffer, 0, size);
            }

            foreach (IFrameSyncData frameSyncData in _frameSyncDatas)
            {
                frameSyncData.Import(_byteBuffer);
            }
        }


        //debug
        Dictionary<string, string> _debugDictionary = new Dictionary<string, string>();
        internal Dictionary<string, string> ExportDictionary()
        {
            _debugDictionary.Clear();
            foreach (IFrameSyncData frameSyncData in _frameSyncDatas)
            {
                frameSyncData.ExportDebugInfo(_debugDictionary);
            }
            return _debugDictionary;
        }
    }
}
