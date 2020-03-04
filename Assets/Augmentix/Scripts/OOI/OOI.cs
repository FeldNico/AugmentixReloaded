using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using Augmentix.Scripts.OOI.Editor;
#endif
using Augmentix.Scripts.VR;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Video;
#if UNITY_WSA
using Vuforia;
#endif

namespace Augmentix.Scripts.OOI
{
    [RequireComponent(typeof(PhotonView))]
    public class OOI : MonoBehaviourPunCallbacks
    {
        [Flags]
        public enum InteractionFlag
        {
            Highlight = 1,
            Text = 2,
            Video = 4,
            Animation = 8,
            Manipulate = 16,
            Scale = 32,
            Changeable = 64,
            Lockable = 128
        }

        public bool StaticOOI = false;
#if UNITY_EDITOR
        [OOIViewEditor.EnumFlagsAttribute]
#endif
        public InteractionFlag Flags = InteractionFlag.Highlight | InteractionFlag.Lockable;

        [TextArea(15, 20)] public string Text;

        private Collider _collider = null;
        private InteractionSphere _interactionSphere;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
            {
                #if UNITY_WSA
                _interactionSphere =
                    Instantiate(FindObjectOfType<InteractionManager>().InteractionSpherePrefab, transform.position + new Vector3(0,_collider.bounds.size.y,0),
                        transform.rotation).GetComponent<InteractionSphere>();
                _interactionSphere.OOI = this;
                #endif
            }
        }

        [PunRPC]
        public void Interact(InteractionFlag flag)
        {
            var view = GetComponent<PhotonView>();
            
            
            /*
            if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
                view.RPC("Interact", RpcTarget.Others, flag);
                */

            switch (flag)
            {
                case InteractionFlag.Highlight:
                {
                    /*
                    if (VRUI.Instance)
                    {
                        VRUI.Instance.ToggleHighlightTarget(gameObject);
                    }
                    else
                    {
                        OOIUI.Instance.ToggleHighlightTarget(gameObject);
                    }
                    */

                    break;
                }
                case InteractionFlag.Video:
                {
                    ToggleVideo();
                    break;
                }
                case InteractionFlag.Text:
                {
                    ToggleText();
                    break;
                }
            }
        }

        private Coroutine _moveText;
        private Dictionary<PlayerAvatar, OOIInfo> _textObjects = new Dictionary<PlayerAvatar, OOIInfo>();

        private void ToggleText()
        {
            
            
            #if UNITY_WSA
            if (_textObjects.Keys.Count == 0)
            {
                foreach (var avatar in PlayerAvatar.SecondaryAvatars)
                {
                    var text = PhotonNetwork.Instantiate(
                        "OOI" + Path.DirectorySeparatorChar + "Info" + Path.DirectorySeparatorChar +
                        FindObjectOfType<InteractionManager>().TextPrefab.name, transform.position,
                        transform.rotation, (byte) TargetManager.Groups.PLAYERS, new object[] {photonView.ViewID});
                    _textObjects[avatar] = text.GetComponent<OOIInfo>();
                }

                _moveText = StartCoroutine(MoveObject(_textObjects, Quaternion.identity,
                    new Vector3(0, -0.2f, 0)));
            }
            else
            {
                StopCoroutine(_moveText);
                foreach (var info in _textObjects.Values)
                {
                    PhotonNetwork.Destroy(info.GetComponent<PhotonView>());
                }
                _textObjects.Clear();
            }
#endif
        }

        private Coroutine _moveVideo = null;
        private Dictionary<PlayerAvatar, OOIInfo> _videoObjects = new Dictionary<PlayerAvatar, OOIInfo>();

        private void ToggleVideo()
        {
            if (PlayerAvatar.SecondaryAvatars.Count == 0)
                return;
            
            #if UNITY_WSA
            if (_videoObjects.Keys.Count == 0)
            {
                foreach (var avatar in PlayerAvatar.SecondaryAvatars)
                {
                    var video = PhotonNetwork.Instantiate(
                        "OOI" + Path.DirectorySeparatorChar + "Info" + Path.DirectorySeparatorChar +
                        FindObjectOfType<InteractionManager>().VideoPrefab.name, transform.position, transform.rotation,
                        (byte) TargetManager.Groups.PLAYERS, new object[] {photonView.ViewID});
                    var scale = video.transform.localScale;
                    scale.y *= (1f * GetComponent<VideoPlayer>().height) / GetComponent<VideoPlayer>().width;
                    transform.localScale = scale;
                    _videoObjects[avatar] = video.GetComponent<OOIInfo>();
                }

                _moveVideo = StartCoroutine(MoveObject(_videoObjects, Quaternion.Euler(0, 0, 180),
                    new Vector3(0, -0.4f, 0)));
            }
            else
            {
                StopCoroutine(_moveVideo);
                foreach (var info in _videoObjects.Values)
                {
                    PhotonNetwork.Destroy(info.GetComponent<PhotonView>());
                }
                _videoObjects.Clear();
            }
        #endif
        }

        private IEnumerator MoveObject(Dictionary<PlayerAvatar, OOIInfo> objects, Quaternion rotationOffset,
            Vector3 positionOffset)
        {
            while (true)
            {
                foreach (var avatar in objects.Keys)
                {
                    var objTransform = objects[avatar].transform;
                    var playerTransform = avatar.transform;
                    objTransform.position = transform.position;
                    var playerPosition = playerTransform.position;
                    
                    Vector3 nearestPoint = _collider.ClosestPoint(playerPosition);

                    nearestPoint.y = playerPosition.y;

                    var newPosition = Vector3.zero;

                    if ((nearestPoint - playerPosition).sqrMagnitude < 1)
                    {
                        newPosition += playerPosition +
                                       Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized *
                                       playerTransform.lossyScale.x;
                    }
                    else
                    {
                        newPosition += playerPosition + (nearestPoint - playerPosition).normalized *
                                       playerTransform.lossyScale.x;
                    }

                    newPosition = newPosition + objTransform.lossyScale.x * positionOffset.y * objTransform.up;
                    objTransform.position = Vector3.Lerp(objTransform.position, newPosition, 0.05f);
                    objTransform.LookAt(new Vector3(playerPosition.x, newPosition.y, playerPosition.z));
                    objTransform.Rotate(Vector3.up, 180);
                    objTransform.rotation = objTransform.rotation * rotationOffset;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}