using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class Moveable : AbstractInteractable
{
    
    new void Start()
    {
        base.Start();

        OnInteractionStart += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint == null)
            {
                joint = gameObject.AddComponent<FixedJoint>();
            }
            joint.connectedBody = hand.PinchingSphere.GetComponent<Rigidbody>();
        };

        OnInteractionEnd += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint != null && joint.connectedBody == hand.PinchingSphere.GetComponent<Rigidbody>())
            {
                Destroy(joint);
            }
        };
        
    }
}
