using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.OOI;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Video;


public class OOIInfo : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public enum InfoType
    {
        Text,
        Video
    }

    public InfoType Type;

    private OOI _ooi;
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var ownerID = (int) info.photonView.InstantiationData[0];

        if (PhotonNetwork.LocalPlayer.ActorNumber == ownerID || TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            var viewID = (int) info.photonView.InstantiationData[1];
            _ooi = PhotonView.Find(viewID).GetComponent<OOI>();
            switch (Type)
            {
                case InfoType.Text:
                {
                    GetComponent<TextMeshPro>().text = _ooi.Text;
                    break;
                }
                case InfoType.Video:
                {
                    var video = _ooi.GetComponent<VideoPlayer>();
                    video.targetMaterialRenderer = GetComponent<Renderer>();
                    video.Play();
                    break;
                }
            }
        }
        else
        {
            foreach (var child in GetComponentsInChildren<Renderer>(true))
            {
                child.enabled = false;
            }
            foreach (var child in GetComponentsInChildren<Collider>(true))
            {
                child.enabled = false;
            }
        }
        
        
    }
}
