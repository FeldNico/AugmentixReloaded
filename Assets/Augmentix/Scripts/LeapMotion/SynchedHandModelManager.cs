using System;
using System.Net;
using System.Net.Sockets;
using Augmentix.Scripts.AR;
using ExitGames.Client.Photon;
using Leap;
using Leap.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
#if UNITY_WSA
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.LeapMotion
{
    public class SynchedHandModelManager : HandModelManager, IPunInstantiateMagicCallback
    {
#if UNITY_WSA
        Vector3 _estimatedHandPosition = new Vector3();
        private HandModel _rightHand, _leftHand;
        private Transform _leapOffset;

        private GameObject leapMotionPalm, leapMotionWrist, hololenPalm;
#elif UNITY_STANDALONE_WIN
        [HideInInspector]
        public bool DoSynchronize = false;
#endif
        private void Awake()
        {
            graphicsEnabled = false;
            physicsEnabled = true;

#if UNITY_WSA
            _rightHand = (HandModel) ModelPool[0].RightModel;
            _leftHand = (HandModel) ModelPool[0].LeftModel;
            _leapOffset = ((ARTargetManager) TargetManager.Instance).LeapMotionOffset;


            leapMotionPalm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leapMotionPalm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            leapMotionPalm.GetComponent<Renderer>().material.color = Color.blue;

            leapMotionWrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leapMotionWrist.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            leapMotionWrist.GetComponent<Renderer>().material.color = Color.yellow;

            hololenPalm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hololenPalm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            hololenPalm.GetComponent<Renderer>().material.color = Color.green;


            InteractionManager.InteractionSourceDetected += args =>
            {
                if (_leftHand.gameObject.activeSelf ^ _rightHand.gameObject.activeSelf)
                {
                    Debug.Log("Pose estimated");
                    args.state.sourcePose.TryGetPosition(out _estimatedHandPosition);
                    var activeHand = _leftHand.gameObject.activeSelf ? _leftHand : _rightHand;
                    var leapPalmPosition =
                        _leapOffset.TransformPoint(activeHand.GetLeapHand().PalmPosition.ToVector3());
                    _leapOffset.position -= leapPalmPosition - _estimatedHandPosition;

                    hololenPalm.transform.position = _estimatedHandPosition;
                }
            };
#endif
        }

        private void Update()
        {
#if UNITY_WSA
            if (_leftHand.gameObject.activeSelf || _rightHand.gameObject.activeSelf)
            {
                leapMotionPalm.transform.localPosition =
                    _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand()
                        .PalmPosition.ToVector3());
                leapMotionWrist.transform.localPosition =
                    _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand()
                        .WristPosition.ToVector3());
            }
#endif
        }

        private void FixedUpdate()
        {
#if UNITY_STANDALONE_WIN
            if (DoSynchronize && CurrentFrame != null)
            {
                ((LeapMotionManager)TargetManager.Instance).SendFrame(CurrentFrame);
            }
#endif
        }

        protected new virtual void OnEnable()
        {
            if (TargetManager.Instance.Type == TargetManager.PlayerType.LeapMotion)
                base.OnEnable();
        }

        protected new virtual void OnDisable()
        {
            if (TargetManager.Instance.Type == TargetManager.PlayerType.LeapMotion)
                base.OnDisable();
        }

#if UNITY_WSA
        public void OnFrameReceived(Frame frame)
        {
            CurrentFrame = frame;
            Debug.Log("Frame " + CurrentFrame.Hands.Count);
            if (graphicsEnabled)
                OnUpdateFrame(CurrentFrame);
            else if (physicsEnabled)
                OnFixedFrame(CurrentFrame);
        }
#endif

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
#if UNITY_WSA
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                var targetManager = ((ARTargetManager) TargetManager.Instance);
                targetManager.HandManager = this;
                transform.parent = targetManager.LeapMotionOffset;
                transform.localPosition = Vector3.zero;

                var ipAdress = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAdress = ip.ToString();
                        break;
                    }
                }

                var port = ((ARTargetManager) TargetManager.Instance).Port;
                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.SEND_IP, new object[] {ipAdress, port},
                    new RaiseEventOptions {Receivers = ReceiverGroup.Others}, new SendOptions {Reliability = true});
            }
#endif
        }
    }
}