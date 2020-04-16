using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class CustomGrabbable : OVRGrabbable
{

    private PhotonView _view;
    private OOI _ooi;

    new void Awake()
    {
        m_grabPoints = GetComponentsInChildren<Collider>();
        base.Awake();
    }
    
    protected override void Start()
    {
        base.Start();
        _view = GetComponent<PhotonView>();
        _ooi = GetComponent<OOI>();
    }

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        _ooi.IsBeingManipulated = true;
        if (_view)
        {
            if (!_view.IsMine)
            {
                _view.RequestOwnership();
            }
        }
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        _ooi.IsBeingManipulated = false;
    }
}
