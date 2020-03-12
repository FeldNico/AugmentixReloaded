using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class PlayerAvatar : MonoBehaviour, IPunInstantiateMagicCallback
{
    public static List<PlayerAvatar> SecondaryAvatars { private set; get; } = new List<PlayerAvatar>();
    public static PlayerAvatar PrimaryAvatar { private set; get; } = null;
    public static PlayerAvatar Mine { private set; get; } = null;

    public static UnityAction<PlayerAvatar> AvatarCreated;
    public static UnityAction<PlayerAvatar> AvatarLost;
    
    private Deskzone _deskzone;
    private TargetManager.PlayerType Type;
    private void Start()
    {
        if (GetComponent<PhotonView>().IsMine)
            Mine = this;
        
        if ((string) GetComponent<PhotonView>().Owner.CustomProperties["Class"] == TargetManager.PlayerType.Primary.ToString())
        {
            Type = TargetManager.PlayerType.Primary;
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                _deskzone = FindObjectOfType<Deskzone>();
                _deskzone.Inside += () => { ToggleVisibility(false); };
                _deskzone.Outside += () => { ToggleVisibility(true); };
            }
            PrimaryAvatar = this;
        }
        else
        {
            Type = TargetManager.PlayerType.Secondary;
            SecondaryAvatars.Add(this);
        }
        AvatarCreated?.Invoke(this);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!info.photonView.IsMine)
            transform.parent = FindObjectOfType<VirtualCity>().transform;
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
        AvatarLost?.Invoke(this);
        if (Type == TargetManager.PlayerType.Primary)
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
