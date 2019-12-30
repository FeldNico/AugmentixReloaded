using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
using Augmentix.Scripts.LeapMotion.Networking.Messages;
using ExitGames.Client.Photon;
using Leap;
using Leap.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LeapMotionClient : LMProtocol, IOnEventCallback
{
    public HandModelManager HandManager;
    public bool DoSynchronize { private set; get; }

    private UDPClient _client { get; } = new UDPClient();

    private Dictionary<Chirality, bool> _prevHandStatus { get; } = new Dictionary<Chirality, bool>
        {{Chirality.Left, false}, {Chirality.Right, false}};

    private List<byte[]> _messages { get; } = new List<byte[]>();

    void FixedUpdate()
    {
        if (DoSynchronize)
        {
            if (HandManager.CurrentFrame != null && CheckUpdate())
            {
                var arraySize = 0;
                _messages.Clear();
                if (HandManager.CurrentFrame.Hands.Count != 2)
                {
                    var options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    options.InterestGroup = (byte) TargetManager.Groups.LEAP_MOTION;

                    if ((HandManager.CurrentFrame.Hands.Count == 0 || HandManager.CurrentFrame.Hands[0].IsLeft) &&
                        _prevHandStatus[Chirality.Right])
                    {
                        _prevHandStatus[Chirality.Right] = false;
                        PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST, new object[] {true},
                            options, SendOptions.SendReliable);
                        Debug.Log("Raise HAND_LOST Event: Right");
                    }

                    if ((HandManager.CurrentFrame.Hands.Count == 0 || HandManager.CurrentFrame.Hands[0].IsRight) &&
                        _prevHandStatus[Chirality.Left])
                    {
                        _prevHandStatus[Chirality.Left] = false;
                        PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST, new object[] {false},
                            options, SendOptions.SendReliable);
                        Debug.Log("Raise HAND_LOST Event: Left");
                    }
                }

                foreach (var hand in HandManager.CurrentFrame.Hands)
                {
                    _prevHandStatus[hand.IsRight ? Chirality.Right : Chirality.Left] = true;
                    var msg = new LMUpdateMessage
                    {
                        IsRight = hand.IsRight,
                        PinchStrength = hand.PinchStrength,
                        IndexPosition = hand.GetIndex().TipPosition.ToVector3(),
                        ThumbPosition = hand.GetThumb().TipPosition.ToVector3()
                    };
                    var data = msg.ConvertToBytes();
                    arraySize += data.Length;
                    _messages.Add(data);
                }

                if (_messages.Count != 0)
                {
                    var bytes = new byte[arraySize];
                    var i = 0;
                    foreach (var message in _messages)
                    {
                        Buffer.BlockCopy(message,0,bytes,i,message.Length);
                        i += message.Length;
                    }
                    _client.Send(bytes);
                }
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
#if UNITY_STANDALONE_WIN
        switch (photonEvent.Code)
        {
            case (byte) TargetManager.EventCode.SEND_IP:
            {
                var ip = (string) ((object[]) photonEvent.CustomData)[0];
                var port = (int) ((object[]) photonEvent.CustomData)[1];
                _client.Connect(ip, port);
                DoSynchronize = true;
                Debug.Log("Connected to " + ip + ":" + port);
                break;
            }
        }
#endif
    }
}