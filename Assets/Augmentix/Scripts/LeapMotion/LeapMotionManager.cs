using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Augmentix.Scripts;
using Augmentix.Scripts.LeapMotion;
using ExitGames.Client.Photon;
using Leap;
using Leap.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class LeapMotionManager : TargetManager
{
    public Player Primary { private get; set; } = null;

    public bool WaitForPrimary = true;
    public float CheckUpdateRate = 0.5f;
    private SynchedHandModelManager _handManager;

    private Socket _socket { get; } = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private State state = new State();
    private const int bufSize = 64 * 1024;
    
    private class State
    {
        public byte[] buffer = new byte[bufSize];
    }

    new public void Start()
    {
        base.Start();
        
        PhotonPeer.RegisterType(typeof(Frame), 42, Frame.Serialize, Frame.Deserialize);
        
        OnConnection += () =>
        {
            Camera.main.backgroundColor = Color.green;
            _handManager = PhotonNetwork
                .Instantiate("Hand Models", Vector3.zero, Quaternion.identity)
                .GetComponent<SynchedHandModelManager>();
            _handManager.transform.parent = FindObjectOfType<XRHeightOffset>().transform;
            _handManager.leapProvider = FindObjectOfType<LeapProvider>();
        };
    }


    public override void OnJoinedRoom()
    {
        PhotonNetwork.SetInterestGroups((byte)Groups.LEAP_MOTION, true);
        StartCoroutine(CheckForPrimary());

        IEnumerator CheckForPrimary()
        {
            Debug.Log("Waiting for Primary");
            while (true)
            {
                var primary = PhotonNetwork.PlayerListOthers.FirstOrDefault(
                    player => (string) player.CustomProperties["Class"] == PlayerType.Primary.ToString());

                if (primary != null || !WaitForPrimary)
                {
                    Primary = primary;
                    break;
                }

                yield return new WaitForSeconds(CheckUpdateRate);
            }

            Debug.Log("Primary found!");
            base.OnJoinedRoom();
        }
    }

    public void SendFrame(Frame frame)
    {
        var data = Frame.Serialize(frame);
        _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            _socket.EndSend(ar);
        }, state);
    }


    public override void OnEvent(EventData photonEvent)
    {
#if UNITY_STANDALONE_WIN
        switch (photonEvent.Code)
        {
            case (byte) EventCode.SEND_IP:
            {
                var ip = (string) ((object[]) photonEvent.CustomData)[0];
                var port = (int) ((object[]) photonEvent.CustomData)[1];
                _socket.Connect(ip,port);
                _handManager.DoSynchronize = true;
                Debug.Log("Connected to "+ip+":"+port);
                break;
            }
            default:
            {
                Debug.Log("Unkown Event recieved: "+photonEvent.Code+" "+photonEvent.CustomData);
                break;
            }
        }
#endif
    }

}