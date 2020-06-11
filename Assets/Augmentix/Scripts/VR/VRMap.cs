using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Photon.Pun;
using UnityEngine;

public class VRMap : MonoBehaviour, IPunInstantiateMagicCallback
{
    public SpriteRenderer MapImage;
    public PointerTarget[] Targets;
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!info.photonView.IsMine && TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            transform.parent = FindObjectOfType<VirtualCity>().transform;
    }
}
