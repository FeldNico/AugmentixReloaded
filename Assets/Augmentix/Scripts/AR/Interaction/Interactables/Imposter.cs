using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Augmentix.Scripts;
using Augmentix.Scripts.Network;
using Augmentix.Scripts.OOI;
using Photon.Pun;
using UnityEngine;

public class Imposter : AbstractInteractable
{
    public GameObject Object;
    
    private void Awake()
    {
        var ooi = GetComponent<OOI>();
        if (ooi)
            Destroy(ooi);
        Destroy(GetComponent<AugmentixTransformView>());
        Destroy(GetComponent<AbstractInteractable>());
        Destroy(GetComponent<PhotonView>());
    }

    // Start is called before the first frame update
    void Start()
    {
        var deskzone = FindObjectOfType<Deskzone>();
        
        OnInteractionStart += (hand) =>
        {
            if (!deskzone.IsInside)
            {
                if (Object == null)
                {
                    Debug.LogError("Object not set");
                    return;
                }

                var obj = PhotonNetwork.Instantiate("OOI"+Path.DirectorySeparatorChar+"Spawnable"+Path.DirectorySeparatorChar+Object.name, transform.position, transform.rotation,
                    (byte) TargetManager.Groups.PLAYERS);
                obj.transform.localScale = transform.lossyScale;

                StartCoroutine(AttachInteractable());
        
                IEnumerator AttachInteractable()
                {
                    yield return new WaitForEndOfFrame();
                    hand.CurrentInteractable = obj.GetComponent<AbstractInteractable>();
                }
            }
        };
        
        Debug.Log("Spawnable"+Path.DirectorySeparatorChar+Object.name);
    }

    /*
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        transform.parent = FindObjectOfType<VirtualCity>().transform;
    }
    */
}
