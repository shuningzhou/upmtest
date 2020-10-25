using UnityEngine;
using System;
using SWNetwork.Core;
using System.Collections.Generic;
using SWNetwork.Core.DataStructure;
using Parallel;

namespace SWNetwork.FrameSync
{
    public abstract class FrameSyncAgent : MonoBehaviour
    {
        public static IFrameSyncDebugger _debugger;
        FrameSyncEngine _engine;
        FrameSyncGame _game;

        bool _debuggerInterupt;

        bool _autoUpdate = true;

        public bool AutoUpdate
        {
            get
            {
                return _autoUpdate;
            }
            set
            {
                _autoUpdate = value;
            }
        }

        public int frameNumber
        {
            get
            {
                if(_game == null)
                {
                    return 0;
                }
                return _game.frameNumber;
            }
        }

        public FrameSyncGameState gameState
        {
            get
            {
                if(_game == null)
                {
                    return FrameSyncGameState.Default;
                }

                return _game.gameState;
            }
        }

        public FrameSyncInput frameSyncInput
        {
            get
            {
                if(_engine == null)
                {
                    return null;
                }

                return _engine._input;
            }
        }

        public FrameSyncEngine frameSyncEngine
        {
            get
            {
                return _engine;
            }
        }

        private void Awake()
        {
            _engine = new FrameSyncEngine();
            
            OnFrameSyncEngineCreated(_engine);

            _engine.SetInputSaveHandler(SaveInput);

            _game = new FrameSyncGame(_engine._input);
            OnFrameSyncGameCreated(_game, null);

            OnFrameSyncGameWillStart();

            StartGame();

            if (_debugger != null)
            {
                _debugger.Initialized(this);
            }
        }

        void OnDestroy()
        {
            StopGame();
        }

        public virtual void Update()
        {
            if (_debuggerInterupt)
            {
                return;
            }

            if (_engine != null)
            {
                OnCollectLocalPlayerInputs(_engine._input, _game);

                bool adjustedTime = _engine.OnUpdate(Time.deltaTime);

                if(adjustedTime)
                {
                    Time.fixedDeltaTime = FrameSyncTime.internalFixedDeltaTime;
                }
            }
        }

        public virtual void FixedUpdate()
        {
            if (_autoUpdate)
            {
                Step();
            }
        }

        float _cachedFixedUpdateTime = 0.02f;

        Fix64 _tickInterval = Fix64.FromDivision(2, 100);

        public void SetTickInterval(Fix64 tickInterval)
        {
            _tickInterval = tickInterval;
        }

        public void StartGame()
        {
            if (_debuggerInterupt)
            {
                return;
            }

            Time.fixedDeltaTime = (float)_tickInterval;
            FrameSyncTime.Initialize(_tickInterval, _game.clientSidePrediction, 0.25f, FrameSyncConstant.DYNAMIC_AVERAGE_COUNT);
            _cachedFixedUpdateTime = Time.fixedDeltaTime;


            _engine.SetFrameSyncGame(_game);
            _engine.Start();
        }

        public void StopGame()
        {
            if (_debuggerInterupt)
            {
                return;
            }

            Time.fixedDeltaTime = _cachedFixedUpdateTime;

            if (_engine != null)
            {
                _engine.Stop();
                _engine.SaveReplay(_lastEndIndex);
                CombineSavedInputFiles();
            }
        }

        public void Step()
        {
            if(_debuggerInterupt)
            {
                return;
            }

            if(_game == null)
            {
                return;
            }

            if (_game.gameState == FrameSyncGameState.Stopped)
            {
                return;
            }
            if(_debugger!=null)
            {
                _debugger.WillStep(_engine, _game);
            }
            _engine.Step(0);
            if (_debugger != null)
            {
                _debugger.DidStep(_engine, _game);
            }
        }

        public void DebugInterupt()
        {
            _debuggerInterupt = true;
        }

        int _lastEndIndex;
        OperationQueue operationQueue = new OperationQueue();

        void SaveInput(SWBytes data, int start, int end)
        {
            SWConsole.Warn($"SaveItems start={start}, end={end}");
            
            if(end > _lastEndIndex)
            {
                _lastEndIndex = end;
            }

            return;
            string partialName = _game.replayFileName + start.ToString("D6") + end.ToString("D6");
            SaveReplayoperation operation = new SaveReplayoperation(data.Data(), partialName);
            operationQueue.AddOperation(operation);
        }

        void CombineSavedInputFiles()
        {
            return;
            string fileName = _game.replayFileName;
            string[] savedFiles = SWLocalStorage.GetFilesWithPartialName(FrameSyncConstant.DEFAULT_DIRECTORY, fileName);

            if(savedFiles.Length == 0)
            {
                return;
            }

            SortedList<int, string> orderedFiles = new SortedList<int, string>();
            List<int> sortedIndexes = new List<int>();

            foreach (string saveFile in savedFiles)
            {
                Debug.Log($"saveFile={saveFile}");
                string frameNumber = saveFile.Substring(saveFile.LastIndexOf(fileName) + fileName.Length);
                Debug.Log($"frameNumber={frameNumber}");
                long frameNumberLong = 0;
                bool good = long.TryParse(frameNumber, out frameNumberLong);
                if (!good)
                {
                    continue;
                }
                int low = (int)(frameNumberLong / 1000000);
                int high = (int)(frameNumberLong % 100000);
                int size = high - low;
                Debug.Log($"low={low} high={high} size={size}");

                orderedFiles[low] = saveFile;
                sortedIndexes.Add(low);
                sortedIndexes.Add(high);
            }

            sortedIndexes.Sort();

            int count = sortedIndexes.Count;
            bool verified = true;
            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                {
                    break;
                }

                if (i % 2 == 1)
                {
                    bool match = sortedIndexes[i] == sortedIndexes[i + 1];
                    if (!match)
                    {
                        verified = false;
                        break;
                    }
                }
            }

            if (!verified)
            {
                Debug.Log("Corrupted files");
            }

            int startFrameNumber = sortedIndexes[0];
            int endFrameNumber = sortedIndexes[sortedIndexes.Count - 1];
            Debug.Log($"start={startFrameNumber} end={endFrameNumber}");
            int estimatedSize = (endFrameNumber - startFrameNumber) * InputFrameDelta.DataSize + 100;
            byte[] dataToSave = new byte[estimatedSize];
            SWBytes combinedBuffer = new SWBytes(dataToSave);
            PersistentArrayMetaData metaData = new PersistentArrayMetaData();
            metaData.itemCount = endFrameNumber - startFrameNumber;
            combinedBuffer.SetWriteIndex(PersistentArrayMetaData.DataSize);

            foreach (var orderedFile in orderedFiles)
            {
                byte[] data;
                int length = SWLocalStorage.LoadFromFile(orderedFile.Value, out data);
                SWBytes dataBuffer = new SWBytes(data);
                //Debug.Log($"Combined add={dataBuffer.FullString()}");
                combinedBuffer.PushAll(dataBuffer);
                Debug.Log($"Remove {orderedFile.Value}");
                SWLocalStorage.RemoveFile(orderedFile.Value);
            }

            combinedBuffer.SetReadIndex(PersistentArrayMetaData.DataSize);
            metaData.checksum = combinedBuffer.Crc32();

            int end = combinedBuffer.GetWriteIndex();
            combinedBuffer.SetWriteIndex(0);
            //Debug.Log($"Combined before meta={combinedBuffer.FullString()}");
            metaData.Export(combinedBuffer);
            //Debug.Log($"Combined after meta={combinedBuffer.FullString()}");
            combinedBuffer.SetWriteIndex(end);
            combinedBuffer.SetReadIndex(0);


            //Debug.Log($"Combined combinedBuffer={combinedBuffer.FullString()}");

            SaveReplayoperation operation = new SaveReplayoperation(combinedBuffer.Data(), fileName + ".play");
            operationQueue.AddOperation(operation);
        }

        public abstract void OnFrameSyncEngineCreated(FrameSyncEngine engine);
        public abstract void OnFrameSyncGameCreated(FrameSyncGame game, SWFrameSyncReplay replay);

        public abstract void OnCollectLocalPlayerInputs(FrameSyncInput input, FrameSyncGame game);

        public virtual void OnFrameSyncGameWillStart()
        {

        }
    }
}
