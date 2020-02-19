using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour, IPunInstantiateMagicCallback
{

    private Deskzone _deskzone;
    private void Start()
    {
        _deskzone = FindObjectOfType<Deskzone>();
        _deskzone.Inside += () => { ToggleVisibility(false); };
        _deskzone.Outside += () => { ToggleVisibility(true); };
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!Equals(info.photonView.Owner, PhotonNetwork.LocalPlayer))
        {
            transform.parent = FindObjectOfType<VirtualCity>().transform;
            if ((string) info.photonView.Owner.CustomProperties["Class"] == TargetManager.PlayerType.Primary.ToString())
                GetComponent<Renderer>().enabled = false;
        }
    }

    public void ToggleVisibility(bool isVisible)
    {
        GetComponent<PhotonView>().RPC("ToogleVisibilityRPC",RpcTarget.Others,isVisible);
    }
    
    [PunRPC]
    private void ToogleVisibilityRPC(bool isVisible)
    {
        GetComponent<Renderer>().enabled = isVisible;
    }
}
