using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
#if UNITY_WSA || UNITY_STANDALONE_WIN
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
    private Dictionary<Chirality, HandStatus> _prevHandStatus { get; } = new Dictionary<Chirality, HandStatus>
        {{Chirality.Left, new HandStatus()}, {Chirality.Right, new HandStatus()}};
    private List<byte[]> _messages { get; } = new List<byte[]>();

    private class HandStatus
    {
        public bool Tracked = false;
        public bool Extended = false;
    }

    private RaiseEventOptions _options = new RaiseEventOptions()
    {
        Receivers = ReceiverGroup.Others,
        InterestGroup = (byte) TargetManager.Groups.LEAP_MOTION
    };

    void FixedUpdate()
    {
        if (DoSynchronize)
        {
            if (HandManager.CurrentFrame != null && CheckUpdate())
            {
                var arraySize = 0;
                if (HandManager.CurrentFrame.Hands.Count != 2)
                {
                    if ((HandManager.CurrentFrame.Hands.Count == 0 || HandManager.CurrentFrame.Hands[0].IsLeft) &&
                        _prevHandStatus[Chirality.Right].Tracked)
                    {
                        _prevHandStatus[Chirality.Right].Tracked = false;
                        /*
                        PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST, true,
                            _options, SendOptions.SendReliable);
                        Debug.Log("Raise HAND_LOST Event: Right");
                        */
                    }

                    if ((HandManager.CurrentFrame.Hands.Count == 0 || HandManager.CurrentFrame.Hands[0].IsRight) &&
                        _prevHandStatus[Chirality.Left].Tracked)
                    {
                        _prevHandStatus[Chirality.Left].Tracked = false;
                        /*
                        PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HAND_LOST, false,
                            _options, SendOptions.SendReliable);
                        Debug.Log("Raise HAND_LOST Event: Left");
                        */
                    }
                }
                

                foreach (var hand in HandManager.CurrentFrame.Hands)
                {
                    var status = _prevHandStatus[hand.IsRight ? Chirality.Right : Chirality.Left];
                    status.Tracked = true;
                    var updatData = new LMUpdateMessage
                    {
                        IsRight = hand.IsRight,
                        PinchStrength = hand.PinchStrength,
                        IndexPosition = hand.GetIndex().TipPosition.ToVector3(),
                        ThumbPosition = hand.GetThumb().TipPosition.ToVector3(),
                        PalmPosition = hand.GetPalmPose().position
                    }.ConvertToBytes();
                    arraySize += updatData.Length;
                    _messages.Add(updatData);

                    if (!hand.GetPinky().IsExtended && !hand.GetRing().IsExtended && !hand.GetMiddle().IsExtended && !hand.GetThumb().IsExtended &&
                        hand.GetIndex().IsExtended)
                    {
                        if (!status.Extended)
                        {
                            status.Extended = true;
                            Debug.Log("Start Pointing");
                        }

                        var pointingData = new LMPointingMessage()
                        {
                            IsRight = hand.IsRight,
                            Direction = hand.GetIndex().Direction.ToVector3()
                        }.ConvertToBytes();

                        arraySize += pointingData.Length;
                        _messages.Add(pointingData);
                    }
                    else
                    {
                        if (status.Extended)
                        {
                            status.Extended = false;
                            
                            Debug.Log("End Pointing");
                        }
                    }
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
                    _messages.Clear();
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
                UpdateFrequenz = (int) ((object[]) photonEvent.CustomData)[2];
                BufferSize = (int) ((object[]) photonEvent.CustomData)[3];
                _client.Connect(ip, port);
                DoSynchronize = true;
                Debug.Log("Connected to " + ip + ":" + port+" with Updatefrequenz "+UpdateFrequenz+" and Buffersize "+BufferSize);
                break;
            }
        }
#endif
    }
}
#endif