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
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
#if UNITY_WSA
        public Transform LeapMotionOffset;
        public Transform AdditionalOffset;
        public int Port = 1337;
        [HideInInspector] public SynchedHandModelManager HandManager = null;

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

        new void Start()
        {
            base.Start();

            PhotonPeer.RegisterType(typeof(Frame), 42, Frame.Serialize, Frame.Deserialize);

            _server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _server.Bind(new IPEndPoint(IPAddress.Any, Port));

            _server.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State) ar.AsyncState;
                _server.EndReceiveFrom(ar, ref epFrom);
                _server.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                _frameStack.Push((Frame) Frame.Deserialize(so.buffer));
            }, state);

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
                    if (frame.Timestamp > _currentTimestamp)
                    {
                        _currentTimestamp = frame.Timestamp;
                        HandManager.OnFrameReceived(frame);
                    }
                }
                else
                {
                    Debug.Log("Dequeue failed");
                }
            }
            else
            {
                Debug.Log("Empty or HandManager == null");
            }
        }
#endif
    }
}