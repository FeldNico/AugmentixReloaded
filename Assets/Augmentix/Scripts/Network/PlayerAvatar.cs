using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.VR;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class PlayerAvatar : MonoBehaviour, IPunInstantiateMagicCallback, IOnEventCallback
{
    public static List<PlayerAvatar> SecondaryAvatars { private set; get; } = new List<PlayerAvatar>();
    public static PlayerAvatar PrimaryAvatar { private set; get; } = null;
    public static PlayerAvatar Mine { private set; get; } = null;

    public static UnityAction<PlayerAvatar> AvatarCreated;
    public static UnityAction<PlayerAvatar> AvatarLost;
    
    private Deskzone _deskzone;
    private VirtualCity _virtualCity;
    private TargetManager.PlayerType Type;
    private LineRenderer _pointer;
    private GameObject _viewCone;
    private PhotonView _view;
    private void Start()
    {
        _view = GetComponent<PhotonView>();
        _virtualCity = FindObjectOfType<VirtualCity>();
        if (_view.IsMine)
            Mine = this;

        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            _deskzone = FindObjectOfType<Deskzone>();
        }
        
        if ((string) GetComponent<PhotonView>().Owner.CustomProperties["Class"] == TargetManager.PlayerType.Primary.ToString())
        {
            Type = TargetManager.PlayerType.Primary;
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                _deskzone.Inside += () => { ToggleVisibility(false); };
                _deskzone.Outside += () => { ToggleVisibility(true); };
            }
        }
        else
        {
            Type = TargetManager.PlayerType.Secondary;
            SecondaryAvatars.Add(this);

            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                var conePrefab = FindObjectOfType<WarpzoneManager>().ViewConePrefab;
                var cone = Instantiate(conePrefab);
                cone.transform.parent = transform;
                cone.transform.localPosition = conePrefab.transform.localPosition;
                cone.transform.localRotation = conePrefab.transform.localRotation;
                cone.transform.localScale = conePrefab.transform.localScale;
                cone.GetComponent<Renderer>().enabled = false;
            }
        }
        AvatarCreated?.Invoke(this);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!info.photonView.IsMine)
        {
            transform.parent = FindObjectOfType<VirtualCity>().transform;
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary && _deskzone.IsInside)
                ToogleVisibilityRPC(false);
        }
        else
        {
            FindObjectOfType<Recorder>().StartRecording();

            Debug.Log("Instantiate Right");
            PhotonNetwork.Instantiate("Hand_R", Vector3.zero, Quaternion.identity);
            Debug.Log("Instantiate Left");
            PhotonNetwork.Instantiate("Hand_L", Vector3.zero, Quaternion.identity);
        }
    }

    public void ToggleVisibility(bool isVisible)
    {
        GetComponent<PhotonView>().RPC("ToogleVisibilityRPC",RpcTarget.Others,isVisible);
    }
    
    [PunRPC]
    private void ToogleVisibilityRPC(bool isVisible)
    {
        transform.Find("Mesh").GetComponent<MeshRenderer>().enabled = isVisible;
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

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case (byte) TargetManager.EventCode.POINTING:
            {
                var data = (object[]) photonEvent.CustomData;
                if (_view.OwnerActorNr == (int) data[0])
                {
                    if (data.Length > 1)
                    {
                        var startPos = _virtualCity.transform.TransformPoint((Vector3) data[1]) ;
                        var endPos = _virtualCity.transform.TransformPoint((Vector3) data[2]);

                        if (_pointer == null)
                        {
                            var go = new GameObject("Pointer");
                            go.transform.parent = transform;
                            go.transform.localPosition = Vector3.zero;
                            _pointer = go.AddComponent<LineRenderer>();
                            _pointer.endWidth = 0.05f;
                            _pointer.startWidth = 0.05f;
                            _pointer.material = FindObjectOfType<VRUI>().PointingMaterial;
                            _pointer.SetPosition(0,startPos);
                            _pointer.SetPosition(1,endPos);
                        }
                        _pointer.SetPosition(0,Vector3.Lerp(_pointer.GetPosition(0),startPos,0.05f));
                        _pointer.SetPosition(1,Vector3.Lerp(_pointer.GetPosition(1),endPos,0.05f));
                        _pointer.enabled = true;
                    }
                    else
                    {
                        if (_pointer != null)
                            _pointer.enabled = false;
                    }
                }
                break;
            }
        }
    }
    
    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
