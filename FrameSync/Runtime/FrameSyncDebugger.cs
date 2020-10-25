using SWNetwork.Core;
using SWNetwork.FrameSync;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSyncDebugFrame
{
    public int frameNumber;
    public float elapsedMS;
    public int playerFrameOnServer;
    public int localServerFrameCount;
    public float inputSampleInterval;
    public float localStepInterval;
    public Dictionary<string, Dictionary<string, string>> inputs;
    public Dictionary<string, Dictionary<string, string>> staticBehaviours;
    public uint hash;

    public FrameSyncDebugFrame()
    {
        inputs = new Dictionary<string, Dictionary<string, string>>();
        staticBehaviours = new Dictionary<string, Dictionary<string, string>>();
    }
}

public class FrameSyncDebugger : MonoBehaviour, IFrameSyncDebugger
{
    FrameSyncAgent _agent;
    SWTCPConnection _tcpConnection;
    SWBytes _tcpTempBytes;

    string _host = "127.0.0.1";
    int _port = 14321;

    void Awake()
    {
        FrameSyncAgent._debugger = this;
        _tcpTempBytes = new SWBytes(16 * 1024);
    }

    void OnDestroy()
    {
        SWConsole.Verbose("FrameSyncDebugger OnDestroy");
        _tcpConnection.OnConnectionEstablished -= _tcpConnection_OnConnectionEstablished;
        _tcpConnection.OnConnectionNewPacket += _tcpConnection_OnConnectionNewPacket;
        _tcpConnection.Stop();
        _tcpConnection = null;
    }

    public void Initialized(FrameSyncAgent agent)
    {
        _tcpConnection = new SWTCPConnection();
        _tcpConnection.OnConnectionEstablished += _tcpConnection_OnConnectionEstablished;
        _tcpConnection.OnConnectionNewPacket += _tcpConnection_OnConnectionNewPacket;
        SWConsole.Verbose("FrameSyncDebugger Initialized");
        _agent = agent;
        _tcpConnection.Connect(_host, _port);
    }

    private void _tcpConnection_OnConnectionEstablished(bool established)
    {
        SWConsole.Verbose($"FrameSyncDebugger: TCP Connection Established={established}");
    }

    private void _tcpConnection_OnConnectionNewPacket(byte[] data, int length)
    {
        SWBytes.Copy(data, 0, length, _tcpTempBytes);
        Util.PrintSWBytes(_tcpTempBytes, "FrameSyncDebugger: New TCP Packet =");
    }

    SWatch _watch = new SWatch();
    bool _watchStarted = false;
    FrameSyncDebugFrame _debugFrame = new FrameSyncDebugFrame();
    SWBytes _outBuffer = new SWBytes(1024 * 16);
    public void WillStep(FrameSyncEngine engine, FrameSyncGame game)
    {
        if (game.gameState == FrameSyncGameState.Running)
        {
            //Debug.LogWarning($"Debugger WillStep {game.frameNumber}");
            _watch.Start();
            _watchStarted = true;
        }
    }

    public void DidStep(FrameSyncEngine engine, FrameSyncGame game)
    {
        if (_watchStarted && game.gameState == FrameSyncGameState.Running)
        {
            _watchStarted = false;
            float ms = _watch.Stop();
            SWConsole.Verbose($"Debugger DidStep {game.frameNumber} ms={ms}");
            //_watcStarted might be in wrong state
            if (game.frameNumber == 0)
            {
                return;
            }

            _debugFrame.frameNumber = game.frameNumber;
            _debugFrame.elapsedMS = ms;
            _debugFrame.playerFrameOnServer = engine.PlayerFrameCountOnServer;
            _debugFrame.localServerFrameCount = engine.LocalServerFrameCount;
            _debugFrame.inputSampleInterval = FrameSyncTime.internalInputSampleInterval * 1000;
            _debugFrame.localStepInterval = FrameSyncTime.internalFixedDeltaTime * 1000;

            SWSystemDataFrame systemDataFrame = engine.GetSystemDataFrame(game.frameNumber);
            _debugFrame.hash = systemDataFrame.bytes.Crc32();
            SWConsole.Verbose($"Debugger DidStep frame hash={_debugFrame.hash}");

            InputFrame inputFrame = engine.GetInputFrame(game.frameNumber);
            FrameSyncInput input = _agent.frameSyncInput;
            FrameSyncInputConfig inputConfig = input.inputConfig;

            _debugFrame.inputs.Clear();

            foreach(FrameSyncPlayer player in input._Players())
            {
                _debugFrame.inputs[player.PlayerID.ToString()] = player.ExportDictionary(inputConfig, inputFrame.bytes);
            }

            _debugFrame.staticBehaviours.Clear();
            List<StaticFrameSyncBehaviour> staticFrameSyncBehaviours = new List<StaticFrameSyncBehaviour>(StaticFrameSyncBehaviourManager._behaviours.Values);

            foreach(StaticFrameSyncBehaviour behaviour in staticFrameSyncBehaviours)
            {
                _debugFrame.staticBehaviours[behaviour.FrameSyncBehaviourID.ToString()] = behaviour.ExportDictionary();
            }

            string json = JSONWriter.ToJson(_debugFrame);
            SWConsole.Verbose(json);

            if(game.type == FrameSyncGameType.Offline)
            {
                SendData(json, 1);
            }
            else
            {
                SendData(json, game.localPlayer.PlayerID);
            }
        }
    }

    public void SendData(string jsonData, byte playerID)
    {
        //length UInt16

        //senderID byte
        //command byte
        //jsonData 2 + jsonData length
        UInt16 length = (UInt16)(1 + 1 + 2 + jsonData.Length);
        _outBuffer.Reset();
        _outBuffer.Push(length);
        _outBuffer.Push(playerID);
        _outBuffer.Push(FrameSyncConstant.DEBUG_SERVER_PLAYER_FRAME);
        _outBuffer.PushUTF8LongString(jsonData);
        _tcpConnection.Send(_outBuffer);
    }
}
