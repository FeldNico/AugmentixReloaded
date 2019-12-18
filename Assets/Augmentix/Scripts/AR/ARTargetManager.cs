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
        
        private Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private const int bufSize = 64 * 1024;
        private AsyncCallback recv;
        private ConcurrentStack<Frame> _frameStack = new ConcurrentStack<Frame>();

        private class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        new public void Awake()
        {
            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!DebugText.text.EndsWith(message+"\n"))
                    DebugText.text += message + "\n";

                if (DebugText.text.Length > 300)
                {
                    DebugText.text = DebugText.text.Substring(DebugText.text.Length - 300, 300);
                }
                
            };
            base.Awake();
        }
        
        new void Start()
        {
            base.Start();

            PhotonPeer.RegisterType(typeof(Frame), 42, Frame.Serialize, Frame.Deserialize);

            Debug.Log("Start Server");
            
            _server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _server.Bind(new IPEndPoint(IPAddress.Any, Port));

            Debug.Log("Started recieve");
            _server.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                Debug.Log("Start Recieved Frame");
                State so = (State) ar.AsyncState;
                _server.EndReceiveFrom(ar, ref epFrom);
                _server.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                _frameStack.Push((Frame) Frame.Deserialize(so.buffer));
                Debug.Log("Recieved Frame");
            }, state);
            Debug.Log("Ended recieve");

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
            if (!_frameStack.IsEmpty && HandManager != null)
            {
                
                var frame = new Frame();
                if (_frameStack.TryPop(out frame))
                {
                    Debug.Log("Process Frame "+frame.Hands.Count);
                    if (frame.Timestamp > _currentTimestamp)
                    {
                        _currentTimestamp = frame.Timestamp;
                        HandManager.OnFrameReceived(frame);
                    }
                }
                else
                {
                    Debug.Log("Pop failed");
                }
            }
            else
            {
                if (HandManager == null)
                    Debug.Log("HandManager == null");
                else
                    Debug.Log("Empty");
            }
        }
#endif
    }
}