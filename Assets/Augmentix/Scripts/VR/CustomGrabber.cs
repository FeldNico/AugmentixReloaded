using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.VR;
using UnityEngine;

public class CustomGrabber : OVRGrabber
{
    private VRTargetManager _targetManager;
    private OVRHand _hand;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        _targetManager = FindObjectOfType<VRTargetManager>();
        _hand = GetComponent<OVRHand>();
    }

    private bool _wasPinching = false;
    // Update is called once per frame
    new void Update()
    {
        base.Update();
        var pinchStrength = _hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        var isPinching = pinchStrength > _targetManager.PinchStrengh;
        
        if (!m_grabbedObj && isPinching != _wasPinching && m_grabCandidates.Count > 0)
        {
            _wasPinching = isPinching;
            GrabBegin();
        }
        if (m_grabbedObj && isPinching != _wasPinching)
        {
            _wasPinching = isPinching;
            GrabEnd();
        }
    }
}
