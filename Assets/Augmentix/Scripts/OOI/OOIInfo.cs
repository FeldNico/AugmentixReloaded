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
        _ooi = PhotonView.Find((int) info.photonView.InstantiationData[0]).GetComponent<OOI>();
        transform.parent = _ooi.transform;
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
}
