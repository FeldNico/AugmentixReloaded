using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
#if UNITY_WSA
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
#endif
using UnityEngine;

public class AugmentixHand : MonoBehaviour, IPunInstantiateMagicCallback
{
    public bool IsRight;
    private PhotonView _view;
    private Transform _transform;
    private Deskzone _deskzone;
    
    void Start()
    {
        _view = GetComponent<PhotonView>();
        _deskzone = FindObjectOfType<Deskzone>();
        _transform = transform;
        Debug.Log("Init "+(IsRight ? "Right" : "Left"));
#if UNITY_ANDROID
        if (_view.IsMine)
        {
            if (IsRight)
            {
                transform.parent = GameObject.Find("RightHandAnchor").transform;
            }
            else
            {
                transform.parent = GameObject.Find("LeftHandAnchor").transform;
            }
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
#endif
    }
    
#if UNITY_WSA
    // Update is called once per frame
    void Update()
    {
        if (_view.IsMine)
        {
            if (!_deskzone.IsInside && HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, IsRight? Handedness.Right : Handedness.Left, out MixedRealityPose pose))
            {
                _transform.localScale = Vector3.one;
                _transform.position = pose.Position;
                _transform.rotation = pose.Rotation;
            }
            else
            {
                _transform.localScale = new Vector3(0.001f,0.001f,0.001f);
            }
        }
    }
#endif
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var avatar = PhotonView.Find((int) info.photonView.InstantiationData[0]).GetComponent<PlayerAvatar>();
        if (IsRight)
            avatar.RightHand = this;
        else
            avatar.LeftHand = this;

        _view = GetComponent<PhotonView>();
        _transform = transform;
        if (_view.IsMine)
        {
            foreach (var child in GetComponentsInChildren<Renderer>())
            {
                child.enabled = false;
            }
        }
        else
        {
            #if UNITY_WSA
            _transform.parent = FindObjectOfType<VirtualCity>().transform;
            #endif
        }

    }
}
