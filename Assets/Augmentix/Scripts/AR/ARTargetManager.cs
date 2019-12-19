using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Augmentix.Scripts.LeapMotion;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_WSA
using Leap;
using Photon.Pun;
using TMPro;
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
#if UNITY_WSA
        public Transform LeapMotionOffset;
        public int Port = 1337;
        [HideInInspector] public SynchedHandModelManager HandManager = null;
        public bool DoCalibrate = false;
        public Vector3 FirstCalibrationVector, SecondCalibrationVector;
        public TMP_Text DebugText;

        public UDPServer Server;

        new public void Awake()
        {
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!DebugText.text.EndsWith(message+"\n"))
                    DebugText.text += message + "\n";

                if (DebugText.text.Length > 2000)
                {
                    DebugText.text = DebugText.text.Substring(DebugText.text.Length - 2000, 2000);
                }
                
            };
            base.Awake();
        }
        
        new void Start()
        {
            base.Start();
            
            Server = new UDPServer(Port);
            //Server.Connect();

            OnConnection += () => { PhotonNetwork.SetInterestGroups((byte) Groups.LEAP_MOTION, true); };
        }
#endif
        public override void OnEvent(EventData photonEvent)
        {
        }
#if UNITY_WSA

        private long _currentTimestamp = 0;
        void FixedUpdate()
        {
            if (HandManager != null && Server.ContainsMessage() && Server.CheckUpdate())
            {
                HandManager.OnFrameReceived(Server.ProcessLatestMessage(bytes => (Frame)Frame.Deserialize(bytes)));
            }
            else
            {
                /*
                if (HandManager == null)
                    Debug.Log("HandManager == null");
                else
                    Debug.Log("Empty");
                    */
            }
        }
#endif
    }
}