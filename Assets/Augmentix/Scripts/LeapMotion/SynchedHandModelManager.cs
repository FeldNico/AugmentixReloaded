using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Augmentix.Scripts.AR;
using Augmentix.Scripts.LeapMotion.Networking.Messages;
using ExitGames.Client.Photon;
using Leap;
using Leap.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
#if UNITY_WSA
using System.Collections.Generic;
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.LeapMotion
{
    public class SynchedHandModelManager : HandModelManager
    {
#if UNITY_WSA
        Vector3 _estimatedHandPosition = new Vector3();
        private HandModel _rightHand, _leftHand;
        private Transform _leapOffset;

        private GameObject leapMotionPalm, leapMotionWrist, hololenPalm, calibratingSphere;
        public CalibrationStateEnum CalibrationState = CalibrationStateEnum.NotCalibrated;

        private Dictionary<CalibrationStateEnum, Vector3> _calibrationVectors =
            new Dictionary<CalibrationStateEnum, Vector3>();

        public enum CalibrationStateEnum
        {
            NotCalibrated,
            FirstCalibrationStep,
            SecondCalibrationStep,
            Calibrated
        }

#elif UNITY_STANDALONE_WIN
        [HideInInspector]
        public bool DoSynchronize = false;

        private LeapMotionManager _leapMotionManager;
#endif

        private void Awake()
        {
            graphicsEnabled = false;
            physicsEnabled = true;

#if UNITY_WSA
            _rightHand = (HandModel) ModelPool[0].RightModel;
            _leftHand = (HandModel) ModelPool[0].LeftModel;
            _leapOffset = ((ARTargetManager) TargetManager.Instance).LeapMotionOffset;

            /*
            leapMotionPalm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leapMotionPalm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            leapMotionPalm.GetComponent<Renderer>().material.color = Color.blue;

            leapMotionWrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leapMotionWrist.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            leapMotionWrist.GetComponent<Renderer>().material.color = Color.yellow;

            hololenPalm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hololenPalm.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            hololenPalm.GetComponent<Renderer>().material.color = Color.red;
            */
            if (((ARTargetManager) TargetManager.Instance).DoCalibrate)
            {
                calibratingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                calibratingSphere.transform.parent = Camera.main.transform;
                calibratingSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                calibratingSphere.transform.localPosition =
                    ((ARTargetManager) TargetManager.Instance).FirstCalibrationVector;
                calibratingSphere.GetComponent<Renderer>().material.color = Color.green;
                CalibrationState = CalibrationStateEnum.FirstCalibrationStep;
            }
            else
            {
                CalibrationState = CalibrationStateEnum.Calibrated;
            }
            
/*
            InteractionManager.InteractionSourceDetected += args =>
            {
                if ((_leftHand.gameObject.activeSelf || _rightHand.gameObject.activeSelf) &&
                    args.state.source.handedness != InteractionSourceHandedness.Unknown &&
                    args.state.sourcePose.positionAccuracy == InteractionSourcePositionAccuracy.High)
                {
                    var activeHand = args.state.source.handedness == InteractionSourceHandedness.Left
                        ? _leftHand
                        : _rightHand;

                    var rot = Quaternion.identity;
                    args.state.sourcePose.TryGetRotation(out rot);
                    
                    Debug.Log("Pose estimated");
                    args.state.sourcePose.TryGetPosition(out _estimatedHandPosition);

                    var leapPalmPosition =
                        _leapOffset.TransformPoint(activeHand.GetLeapHand().Basis.translation.ToVector3());
                    _leapOffset.position -= leapPalmPosition - _estimatedHandPosition;

                    
                    //var leapPalmRotation =
                    //    _leapOffset.TransformRotation(activeHand.GetLeapHand().Basis.rotation.ToQuaternion());
                    //_leapOffset.rotation *= Quaternion.Inverse(leapPalmRotation * Quaternion.Inverse(rot));
                    
                    

                    hololenPalm.transform.position = _estimatedHandPosition;
                }
            };
    */

#elif UNITY_STANDALONE_WIN
            _leapMotionManager = FindObjectOfType<LeapMotionManager>();
#endif
        }

        private bool _waitForReset = false;

        private void Update()
        {
#if UNITY_WSA
            /*
            if (_leftHand.gameObject.activeSelf || _rightHand.gameObject.activeSelf)
            {
                leapMotionPalm.transform.localPosition =
                    _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand()
                        .PalmPosition.ToVector3());
                leapMotionWrist.transform.localPosition =
                    _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand()
                        .WristPosition.ToVector3());
            }
            */

            if (_waitForReset && !_rightHand.GetLeapHand().IsPinching())
                _waitForReset = false;

            if (CalibrationState == CalibrationStateEnum.FirstCalibrationStep)
            {
                if (_rightHand.gameObject.activeSelf && _rightHand.GetLeapHand().IsPinching())
                {
                    Debug.Log("First Pinch detected");
                    _calibrationVectors[CalibrationState] = _rightHand.GetLeapHand().GetPinchPosition();
                    calibratingSphere.transform.localPosition =
                        ((ARTargetManager) TargetManager.Instance).SecondCalibrationVector;
                    CalibrationState = CalibrationStateEnum.SecondCalibrationStep;
                    _waitForReset = true;
                }
            }
            else if (CalibrationState == CalibrationStateEnum.SecondCalibrationStep && !_waitForReset)
            {
                if (_rightHand.gameObject.activeSelf && _rightHand.GetLeapHand().IsPinching())
                {
                    Debug.Log("Second Pinch detected");
                    _calibrationVectors[CalibrationState] = _rightHand.GetLeapHand().GetPinchPosition();
                    Destroy(calibratingSphere);

                    var firstVector = ((ARTargetManager) TargetManager.Instance).FirstCalibrationVector;
                    var firstLeapVector = _calibrationVectors[CalibrationStateEnum.FirstCalibrationStep];
                    var secondVector = ((ARTargetManager) TargetManager.Instance).SecondCalibrationVector;
                    var secondLeapVector = _calibrationVectors[CalibrationStateEnum.SecondCalibrationStep];


                    var scale = Vector3.Distance(firstVector, secondVector) / Vector3.Distance(
                                    firstLeapVector,
                                    secondLeapVector);
                    _leapOffset.transform.localScale = _leapOffset.transform.localScale * scale;
                    Debug.Log("Scaling by "+scale);

                    _leapOffset.transform.localPosition =
                        _leapOffset.transform.localPosition + (firstVector - firstLeapVector);

                    Debug.Log("Offsetting by "+(firstVector - firstLeapVector));
                    
                    var rot = Quaternion.Inverse(Quaternion.FromToRotation(firstVector - secondVector, firstLeapVector - secondLeapVector))
                        .eulerAngles;
                    
                    _leapOffset.transform.RotateAround(firstVector,Vector3.right,rot.x);
                    _leapOffset.transform.RotateAround(firstVector,Vector3.up,rot.y);
                    _leapOffset.transform.RotateAround(firstVector,Vector3.forward,rot.y);

                    Debug.Log("Rotating by "+rot);
                    
                    CalibrationState = CalibrationStateEnum.Calibrated;
                }
            }
#endif
        }

        private Dictionary<bool,bool> _handStatus = new Dictionary<bool, bool>{ {true,false},{false,false} };
        private List<byte[]> _messages = new List<byte[]>();
#if UNITY_STANDALONE_WIN
        private void FixedUpdate()
        {
            if (DoSynchronize)
            {
                if (CurrentFrame != null)
                {
                    _messages.Clear();
                    if (CurrentFrame.Hands.Count != 2)
                    {
                        if (CurrentFrame.Hands.Count == 0)
                        {
                            foreach (var key in _handStatus.Keys)
                                if (_handStatus[key])
                                {
                                    _handStatus[key] = false;
                                    PhotonNetwork.RaiseEvent((byte)TargetManager.EventCode.HAND_LOST, new object[] {key},
                                        RaiseEventOptions.Default,SendOptions.SendReliable);
                                }
                        } else
                        {
                            var hand = CurrentFrame.Hands[0];
                            if (_handStatus[hand.IsRight])
                            {
                                _handStatus[hand.IsRight] = false;
                                PhotonNetwork.RaiseEvent((byte)TargetManager.EventCode.HAND_LOST, new object[] {hand.IsRight},
                                    RaiseEventOptions.Default,SendOptions.SendReliable);
                            }
                        }
                    }
                    
                    foreach (var hand in CurrentFrame.Hands)
                    {
                        var msg = new LMPositionMessage();
                        msg.IsRight = hand.IsRight;
                        msg.IndexPosition = hand.GetIndex().TipPosition.ToVector3();
                        msg.ThumbPosition = hand.GetThumb().TipPosition.ToVector3();
                        _messages.Add(msg.ConvertToBytes());
                    }

                    if (_messages.Count != 0)
                    {
                        var bytes = new byte[0];
                        foreach (var message in _messages)
                        {
                            bytes = bytes.Union(message).ToArray();
                        }
                        
                        
                    }

                }
                else
                {
                    Debug.Log("CurrentFrame == null");
                }
            }
            else
            {
                Debug.Log("No Synchronize");
            }
            
        }
#endif

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
            Debug.Log("frame "+frame.Hands.Count);
            
            if (frame == null)
            {
                Debug.Log("Frame discarded");
                return;
            }
            
            CurrentFrame = frame;
            if (graphicsEnabled)
                OnUpdateFrame(CurrentFrame);
            else if (physicsEnabled)
                OnFixedFrame(CurrentFrame);
        }
#endif
    }
}