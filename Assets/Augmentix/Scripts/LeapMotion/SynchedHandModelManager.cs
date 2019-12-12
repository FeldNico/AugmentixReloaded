using System;
using Augmentix.Scripts.AR;
using Leap;
using Leap.Unity;
using Photon.Pun;
using UnityEngine;
#if UNITY_WSA
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.LeapMotion
{
    public class SynchedHandModelManager : HandModelManager, IPunObservable, IPunInstantiateMagicCallback
    {
#if UNITY_WSA
        Vector3 _estimatedHandPosition = new Vector3();
        private HandModel _rightHand, _leftHand;
        private Transform _leapOffset;

        private GameObject leapMotionPalm,leapMotionWrist, hololenPalm;
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
            leapMotionPalm.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
            leapMotionPalm.GetComponent<Renderer>().material.color = Color.blue;
            
            leapMotionWrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leapMotionWrist.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
            leapMotionWrist.GetComponent<Renderer>().material.color = Color.yellow;
            
            hololenPalm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hololenPalm.transform.localScale = new Vector3(0.05f,0.05f,0.05f);
            hololenPalm.GetComponent<Renderer>().material.color = Color.green;
            */
            
            InteractionManager.InteractionSourceDetected += args =>
            {
                if (_leftHand.gameObject.activeSelf ^ _rightHand.gameObject.activeSelf)
                {
                    Debug.Log("Pose estimated");
                    args.state.sourcePose.TryGetPosition(out _estimatedHandPosition);
                    var activeHand = _leftHand.gameObject.activeSelf ? _leftHand : _rightHand;
                    var leapPalmPosition =
                        _leapOffset.TransformPoint(activeHand.GetLeapHand().PalmPosition.ToVector3()) + ((ARTargetManager)TargetManager.Instance).PalmOffset.localPosition;
                    _leapOffset.position -= leapPalmPosition - _estimatedHandPosition;

                    //hololenPalm.transform.position = _estimatedHandPosition;
                }
            };
#endif
            
        }

        private void Update()
        {
            //leapMotionPalm.transform.localPosition = _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand().PalmPosition.ToVector3()) + ((ARTargetManager)TargetManager.Instance).PalmOffset.localPosition;
            //leapMotionWrist.transform.localPosition = _leapOffset.TransformPoint((_leftHand.gameObject.activeSelf ? _leftHand : _rightHand).GetLeapHand().WristPosition.ToVector3()) + ((ARTargetManager)TargetManager.Instance).PalmOffset.localPosition;
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

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && TargetManager.Instance.Type == TargetManager.PlayerType.LeapMotion)
            {
                if (CurrentFrame != null)
                {
                    var bytes = Frame.Serialize(CurrentFrame);
                    stream.SendNext(bytes);
                }
            }
            else if (stream.IsReading && TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                var bytes = (byte[]) stream.ReceiveNext();
                CurrentFrame = (Frame) Frame.Deserialize(bytes);
                if (graphicsEnabled)
                    OnUpdateFrame(CurrentFrame);
                else if (physicsEnabled)
                    OnFixedFrame(CurrentFrame);
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                transform.parent = ((ARTargetManager) TargetManager.Instance).LeapMotionOffset;
                transform.localPosition = Vector3.zero;
            }
        }
    }
}