using Parallel;
using SWNetwork.Core;
using SWNetwork.Core.DataStructure;
using System;
using System.Collections.Generic;

namespace SWNetwork.FrameSync
{
    public delegate void WillSimulate();
    public class FrameSyncEngine : IFrameSyncHandler, IFrameSyncInputProvider
    {
        string _debugName = "[FrameSyncEngine]";

        private readonly object FRAME_SYNC_LOCK = new object();

        IFrameSyncIO _io;
        FrameSyncGame _game;

        DynamicFrameSyncBehaviourManager _dynamicFrameSyncBehaviourManager = null;
        StaticFrameSyncBehaviourManager _staticFrameSyncBehaviourManager = null;

        public WillSimulate OnEngineWillSimulateEvent;

        SWFrameSyncSystem[] _systems;

        internal FrameSyncInput _input;

        public FrameSyncEngine()
        {
            _staticFrameSyncBehaviourManager = new StaticFrameSyncBehaviourManager();
            _dynamicFrameSyncBehaviourManager = new DynamicFrameSyncBehaviourManager();
            _systems = new SWFrameSyncSystem[2];
            _systems[0] = _staticFrameSyncBehaviourManager;
            _systems[1] = _dynamicFrameSyncBehaviourManager;
        }

        public void SetFrameSyncInputConfig(FrameSyncInputConfig inputConfig)
        {
            _input = new FrameSyncInput(inputConfig);
            _input.SetInputProvider(this);
        }

        public void SetNetworkIO(IFrameSyncIO io)
        {
            _io = io;
            _io.SetFrameSyncHandler(this);
        }

        string _replayFilePath;
        public void SetReplayFilePath(string filePath)
        {
            _replayFilePath = filePath;
        }

        internal void SetFrameSyncGame(FrameSyncGame game)
        {
            _game = game;
        }

        internal void AddFrameSyncSystems(params SWFrameSyncSystem[] systems)
        {
            _systems = systems;
            if (_systems == null)
            {
                _systems = new SWFrameSyncSystem[0];
            }
        }

        public void Stop()
        {
            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Stop();
            }

            _input.SetInputProvider(null);
            _game.gameState = FrameSyncGameState.Stopped;
        }

        public void SaveReplay(int lastSaveEndIndex)
        {
            if (inputFrameDeltas != null)
            {
                inputFrameDeltas.Save(lastSaveEndIndex, _currentInputFrameNumber);
            }
        }

        public void Start()
        {
            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Start();
            }

            _game.gameState = FrameSyncGameState.InitializingRoomFrame;
        }

        float _inputSampleTimer = 0;
        public bool OnUpdate(float deltaTime)
        {
            if(_game.type == FrameSyncGameType.Online && _game.gameState == FrameSyncGameState.Running)
            {
                _inputSampleTimer += deltaTime;
                if(_game.clientSidePrediction)
                {
                    bool adjusted = FrameSyncTime.Adjust(_predictionError, deltaTime);
                    return adjusted;
                }
                else
                {
                    int serverPlayerFrameCount = PlayerFrameCountOnServer;
                    int localServerFrameCount = LocalServerFrameCount;

                    bool adjusted = FrameSyncTime.Adjust(serverPlayerFrameCount, localServerFrameCount, deltaTime);

                    if (_inputSampleTimer > FrameSyncTime.internalInputSampleInterval)
                    {
                        _inputSampleTimer = 0;
                        FlushInputOnline();
                    }

                    return adjusted;
                }
            }

            return false;
        }

        internal int PlayerFrameCountOnServer
        {
            get
            {
                if (_game.type == FrameSyncGameType.Online)
                {
                    return _playerFrameCountOnServer;
                }
                else
                {
                    return 0;
                }
            }  
        }

        internal int LocalServerFrameCount
        {
            get
            {
                if(_game.type == FrameSyncGameType.Online)
                {
                    //received is input delta
                    // inputFrame1 = inputFrame0 + inputDelta0
                    return _lastReceivedInputFrameDeltaNumber + 1 - _currentInputFrameNumber;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void Step(int pingMS)
        {
            lock (FRAME_SYNC_LOCK)
            {
                switch(_game.gameState)
                {
                    case FrameSyncGameState.InitializingRoomFrame:
                        {
                            InitializingRoomFrame();
                            break;
                        }
                    case FrameSyncGameState.WaitingForRoomFrame:
                        {
                            WaitingForRoomFrame();
                            break;
                        }
                    case FrameSyncGameState.WaitingForInitialSystemData:
                        {
                            WaitingForInitialSystemData();
                            break;
                        }
                    case FrameSyncGameState.Running:
                        {
                            if(_game.type == FrameSyncGameType.Offline)
                            {
                                RunningOffline();
                            }
                            else
                            {
                                if(_game.clientSidePrediction == false)
                                {
                                    RunningOnline();
                                }
                                else
                                {
                                    RunningOnlineWithPrediction();
                                }
                            }
                            break;
                        }
                }
            }
        }

        void InitializingRoomFrame()
        {
            if (_game.type == FrameSyncGameType.Offline)
            {
                _game.gameState = FrameSyncGameState.Running;

                _lastReceivedInputFrameDeltaNumber = int.MaxValue - 100; //a large number, -100 to make it don't overflow

                _currentInputFrameNumber = 1;

                InitializeFrames(FrameSyncConstant.DEFAULT_FRAMES_CHUNK_SIZE, 0);
                SetSaveHandler(0);
                //create an empty input frame to start with
                //input frame delta will be created in the next FlushInput 
                inputFrames[_currentInputFrameNumber] = new InputFrame(_currentInputFrameNumber);
            }
            else
            {
                _io.StartReceivingInputFrame();
                _game.gameState = FrameSyncGameState.WaitingForRoomFrame;
                ResetTimeStamp();
            }
        }

        Action<SWBytes, int, int> _saveHandler;
        internal void SetInputSaveHandler(Action<SWBytes, int, int> handler)
        {
            _saveHandler = handler;
        }

        void InitializeFrames(int chunkSize, int startIndex)
        {
            inputFrameDeltas = new PersistentArray<InputFrameDelta>(chunkSize, startIndex);
            predictionInputFrameDeltas = new PersistentArray<InputFrameDelta>(chunkSize, startIndex);
            correctPredictionInputFrameDeltas = new PersistentArray<InputFrameDelta>(chunkSize, startIndex);
            inputFrames = new PersistentArray<InputFrame>(chunkSize, startIndex);
            systemDataFrames = new PersistentArray<SWSystemDataFrame>(chunkSize, startIndex);
            localInputFrameDeltas = new PersistentArray<InputFrameDelta>(chunkSize, 0);
        }

        void SetSaveHandler(int skipSaveIndex)
        {
            SWConsole.Warn($"SetSaveHandler skipSaveIndex={skipSaveIndex}");
            if (_game.replayFileName != null)
            {
                if (_saveHandler != null)
                {
                    inputFrameDeltas.SetSaveDataHandler(_saveHandler, InputFrameDelta.DataSize);
                    inputFrameDeltas.SetSkipSaveIndex(skipSaveIndex);
                }
            }
        }

        void WaitingForRoomFrame()
        {
            if(_firstFrameReceived > 0)
            {
                SWConsole.Crit($"WaitingForRoomFrame _firstFrameReceived={_firstFrameReceived}");
                InputFrameDelta delta = inputFrameDeltas[_firstFrameReceived];

                if(delta != null)
                {
                    SWConsole.Crit($"WaitingForRoomFrame delta not null Delta.frameNumber = {delta.frameNumber}");
                    if (delta.frameNumber == _firstFrameReceived)
                    {
                        if (_firstFrameReceived > 1)
                        {
                            _game.gameState = FrameSyncGameState.WaitingForInitialSystemData;
                            SWConsole.Crit($"WaitingForRoomFrame RequestInputFrames end={_firstFrameReceived}");

                            _io.RequestInputFrames(1, _firstFrameReceived);
                            SWConsole.Crit($"WaitingForRoomFrame game WaitingForInitialSystemData now");
                        }
                        else
                        {
                            //start from 1st frame
                            _currentInputFrameNumber = 1;

                            //create an empty input frame to start with
                            inputFrames[_currentInputFrameNumber] = new InputFrame(_currentInputFrameNumber);
                            _game.gameState = FrameSyncGameState.Running;
                            SetSaveHandler(0);
                            SWConsole.Crit($"WaitingForRoomFrame game running now");
                        }

                        ResetTimeStamp();
                        return;
                    }
                }
            }
            if (CheckInterval(FrameSyncConstant.SERVER_FRAME_INITIALIZATION_INTERVAL))
            {
                SWBytes buffer = new SWBytes(32);

                buffer.Push(0); //frame number
                buffer.Push(0); //predict
                byte length = 0;
                buffer.Push(length);
                _io.SendInputFrameDeltas(buffer, 1, _input.Size);
            }
        }

        void WaitingForInitialSystemData()
        {
            if (HasNewInitialInputFrameDeltas())
            {
                //play all initial input frame
                SWConsole.Crit($"WaitingForInitialSystemData has initial input deltas startFrameNumber={_startFrameNumber}");
                InputFrame inputFrame1 = new InputFrame();
                InputFrame inputFrame2 = new InputFrame();

                int frameNumber = _startFrameNumber + 1; //if start number is 1 delta, we need to simulate 2 because 2 = 1 input + 1 delta
                foreach(InputFrameDelta delta in _initialInputFrameDeltas)
                {
                    inputFrame2.ResetBytes();
                    delta.Apply(_input, inputFrame1, inputFrame2);

                    FrameSyncUpdateType updateType = FrameSyncUpdateType.Restore;

                    //Input manager facing frame data
                    _currentInputFrame = inputFrame2;
                    //user facing frame number
                    _game.frameNumber = frameNumber;

                    SWConsole.Crit($"WaitingForInitialSystemData simulate {frameNumber}");

                    foreach (SWFrameSyncSystem system in _systems)
                    {
                        system.WillUpdate();
                    }

                    foreach (SWFrameSyncSystem system in _systems)
                    {
                        system.Update(_game, _input, updateType);
                    }

                    InputFrame temp = inputFrame1;
                    inputFrame1 = inputFrame2;
                    inputFrame2 = temp;
                    frameNumber++;
                }

                //start from the last restored frame;
                frameNumber--;
                _currentInputFrameNumber = frameNumber;
                ExportSimulationResult();
                //create an empty input frame to start with
                inputFrames[frameNumber] = inputFrame1;
                //export system data
                ExportSimulationResult();
                SWConsole.Warn($"WaitingForInitialSystemData _initialInputFramesData={_initialInputFramesData.DataLength}");
                _saveHandler(_initialInputFramesData, _startFrameNumber, _endFrameNumber);
                _game.gameState = FrameSyncGameState.Running;

                SetSaveHandler(_endFrameNumber - 1); //end frame was excluded from initial frames, so we want to save it
                SWConsole.Crit($"WaitingForInitialSystemData game is running now _currentInputFrameNumber={_currentInputFrameNumber}");
                ResetTimeStamp();
                return;
            }
        }

        //for local games
        void RunningOffline()
        {
            FlushInputOffline();

            if (Simulation())
            {
                ExportSimulationResult();
            }
        }

        void RunningOnline()
        {
            SWConsole.Info($"Engine: ================RunningOnline {_currentInputFrameNumber + 1}=================");
            if (Simulation())
            {
                ExportSimulationResult();
            }
            SWConsole.Info("Engine: ================end=================");
        }

        void RunningOnlineWithPrediction()
        {
            SWConsole.Info("Engine: ================RunningOnlineWithPrediction=================");
            RestoreToConfirmedFrame();

            if (_predictInputFrameDeltaNumber == 0)
            {
                //initialize prediction frame number;
                float halfRTT = _io.Ping() / 2;
                float framesForHalfRTT = halfRTT / (float)FrameSyncTime.fixedDeltaTime;
                int ceiling = (int)Math.Ceiling(framesForHalfRTT);

                _predictInputFrameDeltaNumber = _lastReceivedInputFrameDeltaNumber + ceiling + 1;
            }
            else
            {
                _predictInputFrameDeltaNumber++;
            }

            FlushInputOnlinePrediction();

            int _lastPredictionFrameNumber = _predictInputFrameDeltaNumber - FrameSyncConstant.PREDICTION_GLOBAL_DEBAY_FRAMES;
            if(_lastPredictionFrameNumber < _lastReceivedInputFrameDeltaNumber)
            {
                _lastPredictionFrameNumber = _lastReceivedInputFrameDeltaNumber;
            }
            SWConsole.Info($"Engine: _lastPredictionFrameNumber={_lastPredictionFrameNumber} _predictInputFrameDeltaNumber={_predictInputFrameDeltaNumber}");
            for(int i = _currentInputFrameNumber + 1; i <= _lastPredictionFrameNumber + 1; i++)
            {
                //try use server frames first;
                if(Simulation())
                {
                    ExportSimulationResult();
                    continue;
                }
                else
                {
                    //use predicted frames
                    Predict(i);
                }
            }
            SWConsole.Info("Engine: ================end=================");
        }

        void RestoreToConfirmedFrame()
        {
            //skip the first frame because there is no systemData to restore to
            if(_currentInputFrameNumber > 1)
            {
                SWConsole.Crit($"Engine: RestoreToConfirmedFrame {_currentInputFrameNumber}");
                SWSystemDataFrame systemDataFrame = systemDataFrames[_currentInputFrameNumber];
                ReloadSystemDataSnapshot(systemDataFrame.bytes);
                systemDataFrame.bytes.SetReadIndex(0);
            }
        }

        void FlushInputOffline()
        {
            //write directly to inputFrameDeltas
            InputFrameDelta inputFrameDelta = inputFrameDeltas[_currentInputFrameNumber];
            if (inputFrameDelta == null)
            {
                inputFrameDelta = new InputFrameDelta(_currentInputFrameNumber);
                inputFrameDeltas[_currentInputFrameNumber] = inputFrameDelta;
            }
            inputFrameDelta.frameNumber = _currentInputFrameNumber;

            if (_game.type == FrameSyncGameType.Offline)
            {
                inputFrameDelta.isSealed = true;
            }

            inputFrameDelta.ResetBytes();
            _input.ExportInput(inputFrameDelta.bytes, true);
        }

        public void FlushInputOnline()
        {
            InputFrameDelta previousInputDelta = localInputFrameDeltas[_currentLocalInputFrameDeltaNumber];

            _currentLocalInputFrameDeltaNumber++;

            InputFrameDelta inputFrameDelta = localInputFrameDeltas[_currentLocalInputFrameDeltaNumber];
            if (inputFrameDelta == null)
            {
                inputFrameDelta = new InputFrameDelta(_currentLocalInputFrameDeltaNumber);
                localInputFrameDeltas[_currentLocalInputFrameDeltaNumber] = inputFrameDelta;
            }

            inputFrameDelta.frameNumber = _currentLocalInputFrameDeltaNumber;
            inputFrameDelta.resend = FrameSyncConstant.LOCAL_INPUT_FRAME_RESEND_COUNT;
            inputFrameDelta.ResetBytes();
            _input.ExportInput(inputFrameDelta.bytes, false);

            bool inputChanged = false;
            if (previousInputDelta == null)
            {
                inputChanged = true;
            }
            else
            {
                bool sameInput = previousInputDelta.IsSameInput(inputFrameDelta);
                inputChanged = !sameInput;
            }

            if (!inputChanged)
            {
                SWConsole.Crit("Engine: Input did NOT Change");
                _currentLocalInputFrameDeltaNumber--;
            }
            else
            {
                SWConsole.Crit("Engine: Input Changed");
            }

            SendLocalInputs();
        }

        InputFrameDelta _EMPTY_INPUT_FRAME_DELTA = new InputFrameDelta();
        void FlushInputOnlinePrediction()
        {
            InputFrameDelta previousInputDelta = localInputFrameDeltas[_currentLocalInputFrameDeltaNumber];

            _currentLocalInputFrameDeltaNumber++;

            InputFrameDelta inputFrameDelta = localInputFrameDeltas[_currentLocalInputFrameDeltaNumber];
            if (inputFrameDelta == null)
            {
                inputFrameDelta = new InputFrameDelta(_currentLocalInputFrameDeltaNumber);
                localInputFrameDeltas[_currentLocalInputFrameDeltaNumber] = inputFrameDelta;
            }

            inputFrameDelta.frameNumber = _currentLocalInputFrameDeltaNumber;
            inputFrameDelta.resend = FrameSyncConstant.LOCAL_INPUT_FRAME_RESEND_COUNT;
            inputFrameDelta.ResetBytes();
            _input.ExportInput(inputFrameDelta.bytes, false);

            bool inputChanged = false;
            if (previousInputDelta == null)
            {
                inputChanged = true;
            }
            else
            {
                bool sameInput = previousInputDelta.IsSameInput(inputFrameDelta);
                inputChanged = !sameInput;
            }

            inputFrameDelta.predictedServerFrameNumber = _predictInputFrameDeltaNumber;

            if (!inputChanged)
            {
                SWConsole.Crit($"Engine: Input did NOT Change: prediction={_predictInputFrameDeltaNumber}");
                //_currentLocalInputFrameDeltaNumber--;
                //send an empty frame to keep the fixed delta time adjustment running
                inputFrameDelta.ResetBytes();
                predictionInputFrameDeltas[_predictInputFrameDeltaNumber] = inputFrameDelta;
                correctPredictionInputFrameDeltas[_predictInputFrameDeltaNumber] = inputFrameDelta;
            }
            else
            {
                SWConsole.Crit($"Engine: Input Changed prediction={_predictInputFrameDeltaNumber}");
                predictionInputFrameDeltas[_predictInputFrameDeltaNumber] = inputFrameDelta;
                correctPredictionInputFrameDeltas[_predictInputFrameDeltaNumber] = inputFrameDelta;
            }

            SendLocalInputs();
        }

        SWBytes _sendLocalInputDeltaBuffer = new SWBytes(512);
        void SendLocalInputs()
        {
            if(_localInputFrameDeltaNumberToSend == 0)
            {
                _localInputFrameDeltaNumberToSend = _currentLocalInputFrameDeltaNumber;
            }

            _sendLocalInputDeltaBuffer.Reset();

            int end = _localInputFrameDeltaNumberToSend + FrameSyncConstant.LOCAL_INPUT_FRAME_RESEND_COUNT;
            if(end > _currentLocalInputFrameDeltaNumber)
            {
                end = _currentLocalInputFrameDeltaNumber;
            }

            int count = 0;
            for(int i = _localInputFrameDeltaNumberToSend; i <= end; i++)
            {
                InputFrameDelta inputFrameDelta = localInputFrameDeltas[i];
                _sendLocalInputDeltaBuffer.Push(inputFrameDelta.frameNumber);
                _sendLocalInputDeltaBuffer.Push(inputFrameDelta.predictedServerFrameNumber);
                byte length = (byte)inputFrameDelta.bytes.DataLength;
                _sendLocalInputDeltaBuffer.Push(length);
                _sendLocalInputDeltaBuffer.PushAll(inputFrameDelta.bytes);

                count++;
                inputFrameDelta.resend = inputFrameDelta.resend - 1;
                if(inputFrameDelta.resend == 0)
                {
                    _localInputFrameDeltaNumberToSend++;
                }
            }

            if(count > 0)
            {
                _io.SendInputFrameDeltas(_sendLocalInputDeltaBuffer, count, _input.Size);
            }
        }

        bool Simulation()
        {
            bool simulated = false;
            if (_game.type == FrameSyncGameType.Offline)
            {
                int nextFrameNumber = _currentInputFrameNumber + 1;
                simulated = Simulate(nextFrameNumber);
                if(simulated)
                {
                    _currentInputFrameNumber = nextFrameNumber;
                }
            }
            else
            {
                int nextFrameNumber = _currentInputFrameNumber + 1;
                simulated = Simulate(nextFrameNumber);
                if (simulated)
                {
                    _currentInputFrameNumber = nextFrameNumber;
                }
            }

            return simulated;
        }


        bool Predict(int frameNumber)
        {
            if(frameNumber > _predictInputFrameDeltaNumber + 1)
            {
                return false;
            }

            SWConsole.Crit($"Engine: Predict frameNumber={frameNumber}");
            InputFrame lastInputFrame = inputFrames[frameNumber - 1];
            InputFrameDelta lastInputFrameDelta = correctPredictionInputFrameDeltas[frameNumber - 1];
            if(lastInputFrameDelta == null)
            {
                //SWConsole.Crit($"Engine: Predict use empty delta");
                lastInputFrameDelta = _EMPTY_INPUT_FRAME_DELTA;
            }
            InputFrame inputFrame = inputFrames[frameNumber];

            if (inputFrame == null)
            {
                inputFrame = new InputFrame(frameNumber);
                inputFrames[frameNumber] = inputFrame;
            }

            inputFrame.FrameNumber = frameNumber;
            inputFrame.ResetBytes();

            lastInputFrameDelta.Apply(_input, lastInputFrame, inputFrame);

            FrameSyncUpdateType updateType = FrameSyncUpdateType.Prediction;

            //Input manager facing frame data
            _currentInputFrame = inputFrame;
            //user facing frame number
            _game.frameNumber = frameNumber;

            //hook for other extermal systems
            //physics engine
            if(OnEngineWillSimulateEvent != null)
            {
                OnEngineWillSimulateEvent();
            }

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.WillUpdate();
            }

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Update(_game, _input, updateType);
            }

            return true;
        }

        bool Simulate(int frameNumber)
        {
            if(frameNumber > _lastReceivedInputFrameDeltaNumber + 1)
            {
                return false;
            }

            SWConsole.Info($"Engine: Simulate frameNumber={frameNumber}");

            InputFrame lastInputFrame = inputFrames[frameNumber - 1];

            InputFrameDelta lastInputFrameDelta = inputFrameDeltas[frameNumber - 1];

            InputFrame inputFrame = inputFrames[frameNumber];

            if(inputFrame == null)
            {
                inputFrame = new InputFrame(frameNumber);
                inputFrames[frameNumber] = inputFrame;
            }

            inputFrame.FrameNumber = frameNumber;
            inputFrame.ResetBytes();

            if(lastInputFrame == null || _input == null || inputFrame == null || lastInputFrameDelta == null)
            {
                SWConsole.Error($"Engine: Simulate input data is nil {lastInputFrame} {_input} {inputFrame} {lastInputFrameDelta}");
            }

            lastInputFrameDelta.Apply(_input, lastInputFrame, inputFrame);

            FrameSyncUpdateType updateType = FrameSyncUpdateType.Normal;

            //Input manager facing frame data
            _currentInputFrame = inputFrame;
            //user facing frame number
            _game.frameNumber = frameNumber;

            //hook for other external systems
            //For example, physics engine
            if (OnEngineWillSimulateEvent != null)
            {
                OnEngineWillSimulateEvent();
            }

            //seed the random number generator
            FrameSyncRandom._internal_seed((UInt32)frameNumber);

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.WillUpdate();
            }

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Update(_game, _input, updateType);
            }

            return true;
        }  

        void ExportSimulationResult()
        {
            SWSystemDataFrame systemDataFrame = systemDataFrames[_currentInputFrameNumber];
            if (systemDataFrame == null)
            {
                systemDataFrame = new SWSystemDataFrame(_currentInputFrameNumber);
                systemDataFrames[_currentInputFrameNumber] = systemDataFrame;
            }
            systemDataFrame.FrameNumber = _currentInputFrameNumber;
            systemDataFrame.ResetBytes();

            TakeSystemDataSnapshot(systemDataFrame.bytes);
        }

        //system data
        PersistentArray<SWSystemDataFrame> systemDataFrames; 
        void ReloadSystemDataSnapshot(SWBytes buffer)
        {
            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Import(buffer);
            }
        }

        void TakeSystemDataSnapshot(SWBytes buffer)
        {
            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Export(buffer);
            }
        }

        DateTime _timeStamp;
        void ResetTimeStamp()
        {
            _timeStamp = DateTime.Now;
            _timeStamp = _timeStamp.AddHours(-1);
        }

        bool CheckInterval(int milliseconds)
        {
            if (_timeStamp == null)
            {
                _timeStamp = DateTime.Now;
                return true;
            }
            else
            {
                TimeSpan diff = DateTime.Now - _timeStamp;
                _timeStamp = DateTime.Now;
                return diff.TotalMilliseconds > milliseconds;
            }
        }

        //ISWFrameSyncHandler
        int _playerFrameCountOnServer = 0;
        int _predictionError = 0;
        int _firstFrameReceived = 0;

        public void HandleInputFrameInBackground(SWBytes inputFrame, int playerFrameCountOnServer, int roomStep, int predictFrameNumber, int correctFrameNumber)
        {
            lock (FRAME_SYNC_LOCK)
            {
                SWConsole.Crit($"Engine: HandleInputFrameInBackground roomStep={roomStep} playerFrameCountOnServer={playerFrameCountOnServer} correctFrameNumber={correctFrameNumber} predictFrameNumber={predictFrameNumber}");

                if (_game.gameState == FrameSyncGameState.Stopped)
                {
                    SWConsole.Crit($"Engine: HandleInputFrameInBackground game stopped");
                    return;
                }

                _playerFrameCountOnServer = playerFrameCountOnServer;
                _predictionError = correctFrameNumber - predictFrameNumber;
                if(_predictionError > 0)
                {
                    try
                    {
                        //check if the error has been corrected
                        InputFrameDelta corrected = correctPredictionInputFrameDeltas[correctFrameNumber];
                        if (corrected != null && corrected.predictedServerFrameNumber == predictFrameNumber)
                        {
                            SWConsole.Info($"Engine: already corrected correctFrameNumber={correctFrameNumber}");
                        }
                        else
                        {
                            for (int i = predictFrameNumber; i < correctFrameNumber; i++)
                            {
                                correctPredictionInputFrameDeltas[i] = null;
                            }

                            int endFrameNumber = _predictInputFrameDeltaNumber + _predictionError;
                            for (int i = correctFrameNumber; i <= endFrameNumber; i++)
                            {
                                InputFrameDelta predictionInputFrameDelta = predictionInputFrameDeltas[i - _predictionError];
                                InputFrameDelta correctInputFrameDelta = correctPredictionInputFrameDeltas[i];

                                if (correctInputFrameDelta == null)
                                {
                                    correctPredictionInputFrameDeltas[i] = predictionInputFrameDelta;
                                }
                                else if (correctInputFrameDelta.predictedServerFrameNumber == predictionInputFrameDelta.predictedServerFrameNumber)
                                {
                                    //already moved
                                    continue;
                                }
                                else
                                {
                                    correctPredictionInputFrameDeltas[i] = predictionInputFrameDelta;
                                }
                            }

                            int newPredictFrameDeltaNumber = _predictInputFrameDeltaNumber + _predictionError;

                            for (int i = _predictInputFrameDeltaNumber + 1; i <= newPredictFrameDeltaNumber; i++)
                            {
                                InputFrameDelta emptyFrameDelta = predictionInputFrameDeltas[i];

                                if (emptyFrameDelta == null)
                                {
                                    emptyFrameDelta = new InputFrameDelta(i);
                                    predictionInputFrameDeltas[i] = emptyFrameDelta;
                                }

                                emptyFrameDelta.predictedServerFrameNumber = i;
                                emptyFrameDelta.ResetBytes();
                            }
                            _predictInputFrameDeltaNumber = newPredictFrameDeltaNumber;
                        }

                    }
                    catch(Exception e)
                    {
                        SWConsole.Error(e);
                    }
                }

                if (_lastReceivedInputFrameDeltaNumber == 0)
                {
                    int startIndex = roomStep - 10;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }

                    InitializeFrames(FrameSyncConstant.DEFAULT_FRAMES_CHUNK_SIZE, startIndex);
                    _lastReceivedInputFrameDeltaNumber = roomStep;

                    InputFrameDelta firstDelta = new InputFrameDelta(roomStep);
                    byte length = inputFrame.PopByte();
                    SWBytes.Copy(inputFrame, firstDelta.bytes, length);
                    inputFrameDeltas[roomStep] = firstDelta;
                    _currentInputFrameNumber = 0; //will be updated in the waiting for room frame state
                    _currentLocalInputFrameDeltaNumber = 0;
                    SWConsole.Crit($"Engine: HandleInputFrameInBackground startIndex={startIndex}");
                    return;
                }

                InputFrameDelta delta = inputFrameDeltas[roomStep];

                if (delta == null)
                {
                    delta = new InputFrameDelta();
                    inputFrameDeltas[roomStep] = delta;
                }

                if (delta.frameNumber == roomStep)
                {
                    SWConsole.Crit($"HandleInputFrameInBackground already has {roomStep}");
                }
                else
                {
                    delta.frameNumber = roomStep;
                    SWConsole.Crit($"HandleInputFrameInBackground copy roomStep={roomStep}");// bytes={inputFrame.FullString()}");
                    byte length = inputFrame.PopByte();

                    SWBytes.Copy(inputFrame, delta.bytes, length);
                }

                SWConsole.Crit($"Engine: HandleInputFrameInBackground roomStep={roomStep} _lastReceivedInputFrameDeltaNumber={_lastReceivedInputFrameDeltaNumber}");

                if (roomStep == _lastReceivedInputFrameDeltaNumber + 1)
                {
                    if(_firstFrameReceived == 0)
                    {   //set firstFrameReceived when we have subsequence room steps
                        _firstFrameReceived = _lastReceivedInputFrameDeltaNumber;
                    }

                    _lastReceivedInputFrameDeltaNumber = roomStep;

                    //check if there is any more received frames
                    bool shouldContinue = true;
                    int nextFrameNumber = roomStep + 1;
                    while (shouldContinue)
                    {
                        InputFrameDelta nextDelta = inputFrameDeltas[nextFrameNumber];

                        if (nextDelta == null)
                        {
                            break;
                        }

                        if (nextDelta.frameNumber != nextFrameNumber)
                        {
                            break;
                        }

                        _lastReceivedInputFrameDeltaNumber = nextFrameNumber;

                        nextFrameNumber++;
                    }
                }
            }
        }

        //reload
        List<InputFrameDelta> _initialInputFrameDeltas = new List<InputFrameDelta>();
        int _startFrameNumber = 0;
        int _endFrameNumber = 0;
        SWBytes _initialInputFramesData;

        void PrepareToReceiveInitialInputFrameDeltas()
        {
            _startFrameNumber = 0;
            _endFrameNumber = 0;
        }

        bool HasNewInitialInputFrameDeltas()
        {
            if(_startFrameNumber != 0)
            {
                return true;
            }

            return false;
        }

        //should include startFrame, include endframe
        public void HandleInputFramesInBackground(SWBytes initialInputFramesData, int startFrameNumber, int endFrameNumber)
        {
            lock (FRAME_SYNC_LOCK)
            {
                if (_game.gameState == FrameSyncGameState.Stopped)
                {
                    return;
                }
                SWConsole.Info($"HandleInputFramesInBackground startFrameNumber={startFrameNumber} endFrameNumber={endFrameNumber}");
                _startFrameNumber = startFrameNumber;
                _endFrameNumber = endFrameNumber;
                _initialInputFrameDeltas.Clear();
                _initialInputFramesData = initialInputFramesData;
                for (int i = startFrameNumber; i < endFrameNumber; i++)
                {
                    InputFrameDelta delta = new InputFrameDelta();
                    byte length = initialInputFramesData.PopByte();
                    initialInputFramesData.PopByteBuffer(delta.bytes, 0, length);
                    _initialInputFrameDeltas.Add(delta);
                }

                int expected = endFrameNumber - startFrameNumber;
                int got = _initialInputFrameDeltas.Count;
                //reset read index, we will save the data to disk later
                _initialInputFramesData.SetReadIndex(0);
                if (expected != got)
                {
                    SWConsole.Error($"HandleInputFramesInBackground got={got} expected={expected}");
                }
            }
        }

        //Input Frame
        PersistentArray<InputFrame> inputFrames;
        PersistentArray<InputFrameDelta> inputFrameDeltas;
        PersistentArray<InputFrameDelta> localInputFrameDeltas;
        PersistentArray<InputFrameDelta> predictionInputFrameDeltas;
        PersistentArray<InputFrameDelta> correctPredictionInputFrameDeltas;

        //
        int _predictInputFrameDeltaNumber = 0;

        //
        int _lastReceivedInputFrameDeltaNumber = 0;

        //
        int _currentInputFrameNumber = 0;
        int _currentLocalInputFrameDeltaNumber = 0;
        int _localInputFrameDeltaNumberToSend = 0;

        //
        //int _confirmedInputFrameDeltaNumber = 0;

        //IFrameSyncInputProvider
        InputFrame _currentInputFrame;
        SWBytes _debugInputFrame;
        public SWBytes CurrentInputFrame
        {
            get
            {
                if(_debugInputFrame != null)
                {
                    return _debugInputFrame;
                }

                return _currentInputFrame.bytes;
            }
        }

        //Debug
        public InputFrame GetInputFrame(int frameNumber)
        {
            InputFrame inputFrame = inputFrames[frameNumber];
            return inputFrame;
        }

        public SWSystemDataFrame GetSystemDataFrame(int frameNumber)
        {
            SWSystemDataFrame systemDataFrame = systemDataFrames[frameNumber];
            return systemDataFrame;
        }

        public void SetSystemData(SWBytes buffer)
        {
            ReloadSystemDataSnapshot(buffer);
        }

        public StaticFrameSyncBehaviourManager GetStaticBehaviourManager()
        {
            return _staticFrameSyncBehaviourManager;
        }

        public DynamicFrameSyncBehaviourManager GetDynamicBehaviourManager()
        {
            return _dynamicFrameSyncBehaviourManager;
        }

        public void DebugStep(SWBytes bytes, int frameNumber)
        {
            _debugInputFrame = bytes;

            FrameSyncUpdateType updateType = FrameSyncUpdateType.Normal;

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.WillUpdate();
            }

            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Update(_game, _input, updateType);
            }

            _debugInputFrame = null;
        }

        public void DebugExport(SWBytes bytes)
        {
            foreach (SWFrameSyncSystem system in _systems)
            {
                system.Export(bytes);
            }
        }

    }
}
