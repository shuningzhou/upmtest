using SWNetwork.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SWNetwork.FrameSync
{
    public abstract class MockNetworkOperation : Operation
    {
        protected IFrameSyncHandler _handler;
        protected int _delay;

        public MockNetworkOperation(IFrameSyncHandler handler, int milliseconds)
        {
            _handler = handler;
            _delay = milliseconds;
        }

        public override void Execute()
        {
            Thread.Sleep(_delay);
            Run();
        }

        public abstract void Run();
    }

    public class MockHandleInputFrameOperaion : MockNetworkOperation
    {
        public MockHandleInputFrameOperaion(IFrameSyncHandler handler, int milliseconds) : base(handler, milliseconds) { }

        public SWBytes inputFrameData;
        public int playerLastInputFrameOnServer;
        public int roomStep;
        public byte version;
        public int sealedFrameNumber;
        public int predictionFrameNumber;
        public int correctFrameNumber;

        public override void Run()
        {
            _handler.HandleInputFrameInBackground(inputFrameData, playerLastInputFrameOnServer, roomStep, predictionFrameNumber, correctFrameNumber);
        }
    }

    public class MockFrameSynIO : IFrameSyncIO
    {
        public enum MockServerState
        {
            Idel,
            Running
        }

        IFrameSyncHandler _handler;
        int _pingMilliseconds = 50;
        MockServerState _state;

        InputFrameDelta[] _inputFrameDeltas = new InputFrameDelta[30 * 60 * 10 * 100];
        //SWInputFrameDelta[] _receivedInputFrameDeltas = new SWInputFrameDelta[30 * 60 * 10 * 100];
        Queue<InputFrameDelta> _receivedInputFrameDeltas = new Queue<InputFrameDelta>();
        Queue<InputFrameDelta> _updatedInputFrameDeltas = new Queue<InputFrameDelta>();

        OperationQueue _operationQueue = new OperationQueue(false);
        int _lastReceivedPlayerFrameNumber = 0;
        int _lastPredictedFrameNumber = 0;
        int _frameNumber;

        public MockFrameSynIO(int pingMilliseconds, float tick)
        {
            _pingMilliseconds = pingMilliseconds;
            _TICK_INTERVAL = tick;
            _state = MockServerState.Idel;
            _frameNumber = 0;
        }

        SWBytes _largeData = new SWBytes(1024 * 64);
        public void RequestInputFrames(int startFrameNumber, int endFrameNumber)
        {
            if(startFrameNumber <= endFrameNumber)
            {
                if(startFrameNumber > 0 && endFrameNumber <= _frameNumber)
                {
                    _largeData.Reset();

                    for(int i = startFrameNumber; i < endFrameNumber; i ++)
                    {
                        InputFrameDelta delta = _inputFrameDeltas[i];
                        if(delta == null)
                        {
                            delta = new InputFrameDelta();
                            delta.frameNumber = 0; //playerFrameNumber
                            delta.version = 0;
                        }

                        delta.Export(_largeData);
                    }

                    _handler.HandleInputFramesInBackground(_largeData, startFrameNumber, endFrameNumber);
                }
            }
        }

        int _predictedFrameNumber = 0;
        int _correctFrameNumber = 0;

        public void SendInputFrameDeltas(SWBytes inputFrameDeltas, int count, byte inputSize)
        {
            for(int i = 0; i< count; i++)
            {
                int playerFrameNumber = inputFrameDeltas.PopInt();
                int predictedFrameNumber = inputFrameDeltas.PopInt();

                byte length = inputFrameDeltas.PopByte();

                if(playerFrameNumber == _lastReceivedPlayerFrameNumber + 1)
                {
                    int correctPredictedFrameNumber = 0;

                    if (predictedFrameNumber != 0)
                    {
                        correctPredictedFrameNumber = _lastPredictedFrameNumber + 1;
                        _predictedFrameNumber = predictedFrameNumber;
                        _correctFrameNumber = correctPredictedFrameNumber;
                        SWConsole.Info($"MOCK: SendInputFrameDeltas playerFrameNumber={playerFrameNumber} correctPredictedFrameNumber={correctPredictedFrameNumber} _predictedFrameNumber={_predictedFrameNumber}");
                    }

                    InputFrameDelta delta = new InputFrameDelta();
                    delta.frameNumber = playerFrameNumber;
                    delta.predictedServerFrameNumber = correctPredictedFrameNumber;
                    inputFrameDeltas.PopByteBuffer(delta.bytes, 0, length);
                    _receivedInputFrameDeltas.Enqueue(delta);
                    _lastReceivedPlayerFrameNumber = playerFrameNumber;
                    _lastPredictedFrameNumber = correctPredictedFrameNumber;
                }
                else
                {
                    SWConsole.Info($"MOCK: SendInputFrameDeltas SKIP playerFrameNumber={playerFrameNumber}");
                    inputFrameDeltas.SkipRead(length);
                }
            }
        }

        public void SetFrameSyncHandler(IFrameSyncHandler handler)
        {
            _handler = handler;
        }

        public void StartReceivingInputFrame()
        {
            _state = MockServerState.Running;
        }

        public void Step(float deltaTime)
        {
            switch(_state)
            {
                case MockServerState.Idel:
                    {
                        break;
                    }
                case MockServerState.Running:
                    {
                        Tick(deltaTime);
                        //DoTick();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        float _tickTimer;
        float _TICK_INTERVAL = 0.03f;
        void Tick(float deltaTime)
        {
            _tickTimer += deltaTime;
            //SWConsole.Crit($"MOCK: TICK={_tickTimer} INTERVAL={_TICK_INTERVAL}");
            if(_tickTimer > _TICK_INTERVAL)
            {
                _tickTimer = 0;
                DoTick();
            }
        }

        SWBytes _data = new SWBytes(128);
        int _sealedFrameNumber = 0;

        void DoTick()
        {
            _frameNumber++;
            _sealedFrameNumber = _frameNumber - 10;
            if (_sealedFrameNumber < 1)
            {
                _sealedFrameNumber = 1;
            }

            if (_receivedInputFrameDeltas.Count > 0)
            {
                SWConsole.Crit($"MockIO: DoTick playerFrameCount={_receivedInputFrameDeltas.Count}");
                InputFrameDelta delta = _receivedInputFrameDeltas.Peek();

                if (delta.predictedServerFrameNumber == 0 || delta.predictedServerFrameNumber <= _frameNumber)
                {
                    delta = _receivedInputFrameDeltas.Dequeue();
                    //SWConsole.Crit($"MockIO: DoTick playerFrameCount 1 ={_receivedInputFrameDeltas.Count}");
                    delta.version = 0;
                    _inputFrameDeltas[_frameNumber] = delta;

                    _data.Reset();

                    byte length = (byte)delta.bytes.DataLength;
                    _data.Push(length);
                    _data.PushAll(delta.bytes);

                    SWConsole.Crit($"MockIO: DoTick send PLAYER={delta.frameNumber} roomStep={_frameNumber}");

                    MockHandleInputFrameOperaion operation = new MockHandleInputFrameOperaion(_handler, _pingMilliseconds);
                    operation.inputFrameData = SWBytes.Clone(_data);
                    operation.playerLastInputFrameOnServer = _receivedInputFrameDeltas.Count;
                    operation.predictionFrameNumber = _predictedFrameNumber;
                    operation.correctFrameNumber = _correctFrameNumber;
                    operation.roomStep = _frameNumber;
                    operation.version = delta.version;
                    operation.sealedFrameNumber = _sealedFrameNumber;
                    _operationQueue.AddOperation(operation);
                    return;
                }
                //else if(delta.predictedServerFrameNumber < _frameNumber)
                //{
                //    delta = _receivedInputFrameDeltas.Dequeue();
                //    delta.version = 1;
                //    _inputFrameDeltas[delta.predictedServerFrameNumber] = delta;

                //    _data.Reset();

                //    byte length = (byte)delta.bytes.DataLength;
                //    _data.Push(length);
                //    _data.Push(delta.bytes, 0);

                //    SWConsole.Crit($"MockIO: DoTick send PLAYER={delta.frameNumber} roomStep={_frameNumber} prediction={delta.predictedServerFrameNumber}");
                //    _frameNumber--;
                //    _sealedFrameNumber = _frameNumber - 10;
                //    if (_sealedFrameNumber < 1)
                //    {
                //        _sealedFrameNumber = 1;
                //    }
                //    MockHandleInputFrameOperaion operation = new MockHandleInputFrameOperaion(_handler, _pingMilliseconds);
                //    operation.inputFrameData = SWBytes.Clone(_data);
                //    operation.playerLastInputFrameOnServer = _receivedInputFrameDeltas.Count;
                //    operation.predictionError = _predictionError;
                //    operation.roomStep = delta.predictedServerFrameNumber;
                //    operation.version = delta.version;
                //    operation.sealedFrameNumber = _sealedFrameNumber;
                //    _operationQueue.AddOperation(operation);
                //    return;
                //}
            }

            {
                _lastPredictedFrameNumber = _frameNumber;
                InputFrameDelta delta = new InputFrameDelta();
                delta.frameNumber = 0; //playerFrameNumber
                delta.version = 0;
                _inputFrameDeltas[_frameNumber] = delta;

                _data.Reset();
                _data.Push((byte)0); //length
                SWConsole.Crit($"MockIO: DoTick send EMPTY={delta.frameNumber} roomStep={_frameNumber}");

                MockHandleInputFrameOperaion operation = new MockHandleInputFrameOperaion(_handler, _pingMilliseconds);
                operation.inputFrameData = SWBytes.Clone(_data);
                operation.playerLastInputFrameOnServer = _receivedInputFrameDeltas.Count;
                operation.predictionFrameNumber = _predictedFrameNumber;
                operation.correctFrameNumber = _correctFrameNumber;
                operation.roomStep = _frameNumber;
                operation.version = delta.version;
                operation.sealedFrameNumber = _sealedFrameNumber;
                _operationQueue.AddOperation(operation);

            }
        }

        public float Ping()
        {
            return (float)_pingMilliseconds / 1000.0f;
        }
    }
}
