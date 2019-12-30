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
public class LeapMotionServer : LMProtocol, IOnEventCallback
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
                    new object[] {_server.IP, _server.Port}, RaiseEventOptions.Default, SendOptions.SendReliable);
                Debug.Log("Sent IP Event " + (byte) TargetManager.EventCode.SEND_IP + " " + _server.IP + ":" +
                          _server.Port);
            });
        };
    }



    private List<TimeOutData> _timeoutCache = new List<TimeOutData>();
    void FixedUpdate()
    {
        if (_server.ContainsMessage())
        {
            var messages = ConvertBytesToMessageArray(_server.GetCurrentMessageArray());
            foreach (var message in messages)
            {
                message.HandleMessage();
                message.HandleTimeout(_timeoutCache);
            }

            foreach (var data in new List<TimeOutData>(_timeoutCache))
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

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte) TargetManager.EventCode.HAND_LOST:
            {
                var isRight = (bool) photonEvent.CustomData;
                var hand = isRight ? _targetManager.Hands.Right : _targetManager.Hands.Left;
                hand.IsDetected = false;
                hand.OnLost?.Invoke();
                break;
            }
            case (byte) TargetManager.EventCode.EXTENDED:
            {
                var isRight = (bool) photonEvent.CustomData;
                var hand = isRight ? _targetManager.Hands.Right : _targetManager.Hands.Left;
                hand.IsPointing = false;
                hand.OnPointEnd?.Invoke();
                break;
            }
        }
    }
}
#endif