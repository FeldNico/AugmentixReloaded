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
        
        Debug.Log("Spawnable"+Path.DirectorySeparatorChar+Object.name);
    }

    private void CreateSpawnable(ARHand hand)
    {
        if (Object == null)
        {
            Debug.LogError("Object not set");
            return;
        }

        var obj = PhotonNetwork.Instantiate("Spawnable"+Path.DirectorySeparatorChar+Object.name, transform.position, transform.rotation,
            (byte) TargetManager.Groups.PLAYERS);
        obj.transform.localScale = transform.lossyScale;
        
        
        hand.CurrentInteractable = obj.GetComponent<AbstractInteractable>();
    }
    
}
