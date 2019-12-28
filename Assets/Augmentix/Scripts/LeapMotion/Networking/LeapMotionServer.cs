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
    
    private UDPServer _server;
    private ARTargetManager _targetManager;

    // Start is called before the first frame update
    void Start()
    {
        _targetManager = (ARTargetManager) TargetManager.Instance;
        _server = new UDPServer(_targetManager.Port);

        TargetManager.Instance.OnConnection += () =>
            {
                StartCoroutine(CheckForSecondary());

                IEnumerator CheckForSecondary()
                {
                    Debug.Log("Waiting for LeapMotion");
                    while (true)
                    {
                        var primary = PhotonNetwork.PlayerListOthers.FirstOrDefault(
                            player => (string) player.CustomProperties["Class"] == TargetManager.PlayerType.LeapMotion.ToString());

                        if (primary != null)
                        {
                            var options = new RaiseEventOptions();
                            options.Receivers = ReceiverGroup.Others;
                            options.InterestGroup = (byte) TargetManager.Groups.LEAP_MOTION;
                            PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.SEND_IP,
                                new object[] {_server.IP, _server.Port}, RaiseEventOptions.Default, SendOptions.SendReliable);
                            Debug.Log("Sent IP Event "+(byte) TargetManager.EventCode.SEND_IP+" "+_server.IP+":"+_server.Port);
                            break;
                        }

                        yield return new WaitForSeconds(CheckUpdateRate);
                    }
                    
                }
                
               
            };
    }

    
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
