using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class Moveable : MonoBehaviour, IPunInstantiateMagicCallback
{
    void Start()
    {
        #if UNITY_WSA
        
#elif UNITY_ANDROID
        gameObject.AddComponent<OVRGrabbable>();
#endif

    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        transform.parent = FindObjectOfType<VirtualCity>().transform;
    }
}
