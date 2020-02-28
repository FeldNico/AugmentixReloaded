using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;

public class Imposter : AbstractInteractable
{
    public GameObject Object;

    // Start is called before the first frame update
    void Start()
    {
        OnInteractionStart += CreateSpawnable;
    }

    private void CreateSpawnable(ARHand hand)
    {
        if (Object == null)
        {
            Debug.LogError("Object not set");
            return;
        }

        var obj = PhotonNetwork.Instantiate("Spawnable"+Path.PathSeparator+Object.name, Object.transform.position, Object.transform.rotation,
            (byte) TargetManager.Groups.PLAYERS);
        
        hand.CurrentInteractable = obj.GetComponent<AbstractInteractable>();
    }
    
}
