using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!Equals(info.photonView.Owner, PhotonNetwork.LocalPlayer))
        {
            transform.parent = FindObjectOfType<VirtualCity>().transform;
        }
    }
}
