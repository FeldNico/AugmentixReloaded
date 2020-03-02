using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerAvatar : MonoBehaviour, IPunInstantiateMagicCallback
{
    public static List<PlayerAvatar> SecondaryAvatars { private set; get; } = new List<PlayerAvatar>();
    public static PlayerAvatar PrimaryAvatar { private set; get; } = null;
    public static PlayerAvatar Mine { private set; get; } = null;
    
    private Deskzone _deskzone;
    private void Start()
    {
        if (GetComponent<PhotonView>().IsMine)
            Mine = this;
        
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            _deskzone = FindObjectOfType<Deskzone>();
            _deskzone.Inside += () => { ToggleVisibility(false); };
            _deskzone.Outside += () => { ToggleVisibility(true); };
            PrimaryAvatar = this;
        }
        else
        {
            SecondaryAvatars.Add(this);
        }
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

    private void OnDestroy()
    {
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            PrimaryAvatar = null;
        }
        else
        {
            if (SecondaryAvatars.Contains(this))
                SecondaryAvatars.Remove(this);
        }
    }
}
