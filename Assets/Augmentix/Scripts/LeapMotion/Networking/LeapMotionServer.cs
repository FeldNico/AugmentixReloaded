using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

#if UNITY_WSA
public class LeapMotionServer : LMProtocol, IOnEventCallback
{

    private UDPServer _server;
    private ARTargetManager _targetManager;

    private void Awake()
    {
        Indexing();
    }

    // Start is called before the first frame update
    void Start()
    {
        _targetManager = (ARTargetManager) TargetManager.Instance;
        _server = new UDPServer(_targetManager.Port);
    }

    
#if UNITY_WSA
    void FixedUpdate()
    {
        if (_server.ContainsMessage())
        {
            var messages = ConvertBytesToMessageArray(_server.GetCurrentMessageArray());
            foreach (var message in messages)
            {
                message.HandleMessage();
            }
        }
    }
#endif
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte) TargetManager.EventCode.HAND_LOST:
            {
                var isRight = (bool) ((object[]) photonEvent.CustomData)[0];
                var hand = isRight ? _targetManager.Hands.Right : _targetManager.Hands.Left;
                hand.OnLost.Invoke();
                break;
            }
        }
    }
}
#endif
