using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

#if UNITY_WSA
public class LeapMotionServer : LMProtocol
{
    public float CheckUpdateRate = 0.5f;
    public float MessageTimeout = 2;

    private UDPServer _server;
    private ARTargetManager _targetManager;
    
    // Start is called before the first frame update
    void Start()
    {
        _targetManager = (ARTargetManager) TargetManager.Instance;
        _server = new UDPServer(_targetManager.Port);
        _server.Connect();
        
        TargetManager.Instance.OnConnection += () =>
        {
            TargetManager.Instance.WaitForPlayer(TargetManager.PlayerType.LeapMotion, CheckUpdateRate, () =>
            {
                var options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;
                options.InterestGroup = (byte) TargetManager.Groups.LEAP_MOTION;
                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.SEND_IP,
                    new object[] {_server.IP, _server.Port,UpdateFrequenz,BufferSize}, RaiseEventOptions.Default, SendOptions.SendReliable);
                Debug.Log("Sent IP Event " + (byte) TargetManager.EventCode.SEND_IP + " " + _server.IP + ":" +
                          _server.Port+" with Updatefrequenz "+UpdateFrequenz+" and Buffersize "+BufferSize);
            });
        };
    }
    
    private List<TimeOutData> _timeoutCache = new List<TimeOutData>();
    private List<TimeOutData> _tmpCache = new List<TimeOutData>();
    private Dictionary<LeapMotionMessageType,ILMMessage> _messageCache = new Dictionary<LeapMotionMessageType, ILMMessage>();
    void FixedUpdate()
    {
        if (CheckUpdate())
        {
            var msg = _server.CurrentMessage;
            if (msg != null)
            {
                var index = 0;
                while (index < msg.Length)
                {
                    var type = (LeapMotionMessageType) msg[index];
                    if (type == LeapMotionMessageType.Invalid || !TypeDict.ContainsKey(type))
                        break;
                    index += sizeof(byte);

                    if (!_messageCache.ContainsKey(type))
                    {
                        _messageCache[type] = (ILMMessage) Activator.CreateInstance(TypeDict[type]);
                    }

                    var message = _messageCache[type];
                    index = message.ConvertFromBytes(msg, index);
                    message.HandleMessage();
                    message.HandleTimeout(_timeoutCache);
                }
                
            }
            _tmpCache.Clear();
            _tmpCache.AddRange(_timeoutCache);
            foreach (var data in _tmpCache)
            {
                if (data.Time > MessageTimeout)
                {
                    data.Message.OnTimeout();
                    _timeoutCache.Remove(data);
                }
                else
                {
                    data.Time++;
                }
            }
            
        }
    }
}
#endif