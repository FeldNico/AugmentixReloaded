using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Moveable : AbstractInteractable, IPunInstantiateMagicCallback
{
    
    new void Start()
    {
        base.Start();

        OnInteractionStart += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint == null)
                joint = gameObject.AddComponent<FixedJoint>();

            joint.connectedBody = hand.PinchingSphere.GetComponent<Rigidbody>();
        };

        OnInteractionEnd += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint != null && joint.connectedBody == hand.PinchingSphere.GetComponent<Rigidbody>())
                Destroy(joint);
        };
        
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        transform.parent = FindObjectOfType<VirtualCity>().transform;
#if UNITY_ANDROID
        var grabber = gameObject.AddComponent<OVRGrabbable>();
#endif
    }
}
