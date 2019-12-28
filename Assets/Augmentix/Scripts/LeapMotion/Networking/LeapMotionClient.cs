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
    private Dictionary<Chirality,bool> _prevHandStatus { get; } = new Dictionary<Chirality, bool>{ {Chirality.Left,false},{Chirality.Right,false} };
    private List<byte[]> _messages  { get; } = new List<byte[]>();

    void FixedUpdate()
    {
        if (DoSynchronize)
        {
            if (HandManager.CurrentFrame != null && CheckUpdate())
            {
                _messages.Clear();
                if (HandManager.CurrentFrame.Hands.Count != 2)
                {
                    var options = new RaiseEventOptions();
                    options.Receivers = ReceiverGroup.Others;
                    options.InterestGroup = (byte) TargetManager.Groups.LEAP_MOTION;
                    
                    if (HandManager.CurrentFrame.Hands.Count == 0)
                    {
                        foreach (var key in _prevHandStatus.Keys)
                            if (_prevHandStatus[key])
                            {
                                _prevHandStatus[key] = false;
                                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST, new object[] {key == Chirality.Right},
                                    options, SendOptions.SendReliable);
                                Debug.Log("Raise HAND_LOST Event: "+ (key == Chirality.Right ? "Right" : "Left" ));
                            }
                    }
                    else
                    {
                        var hand = HandManager.CurrentFrame.Hands[0];
                        if (hand.IsRight)
                        {
                            if (_prevHandStatus[Chirality.Right])
                            {
                                _prevHandStatus[Chirality.Right] = false;
                                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST,
                                    new object[] {hand.IsRight},
                                    options, SendOptions.SendReliable);
                                Debug.Log("Raise HAND_LOST Event: Right" );
                            }
                        }
                        else
                        {
                            if (_prevHandStatus[Chirality.Left])
                            {
                                _prevHandStatus[Chirality.Left] = false;
                                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST,
                                    new object[] {hand.IsRight},
                                    options, SendOptions.SendReliable);
                                Debug.Log("Raise HAND_LOST Event: Left" );
                            }
                        }
                    }
                }

                foreach (var hand in HandManager.CurrentFrame.Hands)
                {
                    _prevHandStatus[hand.IsRight ? Chirality.Right : Chirality.Left] = true;
                    var msg = new LMUpdateMessage();
                    msg.IsRight = hand.IsRight;
                    msg.PinchStrength = hand.PinchStrength;
                    msg.IndexPosition = hand.GetIndex().TipPosition.ToVector3();
                    msg.ThumbPosition = hand.GetThumb().TipPosition.ToVector3();
                    Debug.Log("Added Update Message "+hand.IsRight);
                    _messages.Add(msg.ConvertToBytes());
                }

                if (_messages.Count != 0)
                {
                    var bytes = new byte[0];
                    foreach (var message in _messages)
                    {
                        bytes = bytes.Union(message).ToArray();
                    }
                    _client.Send(bytes);
                }
            }
            else
            {
                Debug.Log("CurrentFrame == null");
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
                _client.Connect(ip,port);
                DoSynchronize = true;
                Debug.Log("Connected to "+ip+":"+port);
                break;
            }
        }
#endif
    }
}