using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.AR;
using Augmentix.Scripts.LeapMotion;
using Leap;
using Leap.Unity;
using Augmentix.Scripts;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class HandModel : HandModelBase
{
    
    [SerializeField]
    private Chirality handedness;

    private Hand _hand;
    private SynchedHandModelManager _handManager = null;
#if UNITY_WSA
    private Transform _leapOffset;
#endif
    private GameObject[] _fingers = new GameObject[5];

    public void Awake()
    {
        _handManager = FindObjectOfType<SynchedHandModelManager>();
#if UNITY_WSA
        _leapOffset = ((ARTargetManager) TargetManager.Instance).LeapMotionOffset;
#endif
        for (int i = 0; i < _fingers.Length; i++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = transform;
            sphere.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
            sphere.transform.localPosition = Vector3.zero;
            _fingers[i] = sphere;
        }
    }

    // Update is called once per frame
    public override ModelType HandModelType {
        get {
            return ModelType.Physics;
        }
    }
    
    public override Chirality Handedness {
        get {
            return handedness;
        }
        set { }
    }
    public override void UpdateHand()
    {
        for (int i = 0; i < _fingers.Length; i++)
        {
#if UNITY_WSA
            if (_handManager.CalibrationState == SynchedHandModelManager.CalibrationStateEnum.Calibrated)
            {
                _fingers[i].transform.position = _leapOffset.transform.TransformPoint( _hand.Fingers[i].TipPosition.ToVector3());
                //_fingers[i].transform.localPosition = _hand.Fingers[i].TipPosition.ToVector3();
            }
#elif UNITY_STANDALONE_WIN
            _fingers[i].transform.position =  _hand.Fingers[i].TipPosition.ToVector3();
#endif
        }

    }

    public override Hand GetLeapHand()
    {
        return _hand;
    }

    public override void SetLeapHand(Hand hand)
    {
        _hand = hand;
    }
    
#if UNITY_EDITOR
    public new void Update() {
        if (!EditorApplication.isPlaying && SupportsEditorPersistence()) {

            Hand hand = null;
            if (TargetManager.Instance.Type == TargetManager.PlayerType.LeapMotion)
            {
                LeapProvider provider = null;

                //First try to get the provider from a parent HandModelManager
                if (transform.parent != null) {
                    var manager = transform.parent.GetComponent<HandModelManager>();
                    if (manager != null) {
                        provider = manager.leapProvider;
                    }
                }

                //If not found, use any old provider from the Hands.Provider getter
                if (provider == null) {
                    provider = Hands.Provider;
                }

                
                //If we found a provider, pull the hand from that
                if (provider != null) {
                    var frame = provider.CurrentFrame;

                    if (frame != null) {
                        hand = frame.Get(Handedness);
                    }
                }

                //If we still have a null hand, construct one manually
                if (hand == null) {
                    hand = TestHandFactory.MakeTestHand(Handedness == Chirality.Left, unitType: TestHandFactory.UnitType.LeapUnits);
                    hand.Transform(transform.GetLeapMatrix());
                }
            } else if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                hand = _handManager.CurrentFrame.Get(Handedness);
            } else if (TargetManager.Instance.Type == TargetManager.PlayerType.Secondary)
            {
                return;
            }
            
            if (GetLeapHand() == null) {
                SetLeapHand(hand);
                InitHand();
                BeginHand();
                UpdateHand();
            } else {
                SetLeapHand(hand);
                UpdateHand();
            }
        }
    }
#endif
}
