﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using Augmentix.Scripts.OOI.Editor;
#endif
using Augmentix.Scripts.VR;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
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
            Manipulate = 8,
            Changeable = 16
        }

        public bool StaticOOI = false;
#if UNITY_EDITOR
        [OOIViewEditor.EnumFlagsAttribute]
#endif
        public InteractionFlag Flags = InteractionFlag.Highlight;

        [TextArea(15, 20)] public string Text;

        public Collider Collider { private set; get; } = null;
        private InteractionSphere _interactionSphere;

        private void Start()
        {
            Collider = GetComponent<Collider>();
#if UNITY_WSA
            var prefab = FindObjectOfType<InteractionManager>().InteractionSpherePrefab;
                
                _interactionSphere =
                    Instantiate(prefab, transform.position + new Vector3(0,Collider.bounds.size.y,0),
                        transform.rotation).GetComponent<InteractionSphere>();
                _interactionSphere.OOI = this;
#endif

            if (Flags.HasFlag(InteractionFlag.Manipulate))
            {
#if UNITY_WSA
                var manipulator = gameObject.AddComponent<ObjectManipulator>();
                manipulator.ReleaseBehavior = 0;
                gameObject.AddComponent<NearInteractionGrabbable>();
                gameObject.AddComponent<MinMaxScaleConstraint>();
                manipulator.OnManipulationStarted.AddListener(data =>
                {
                    GetComponent<PhotonView>().RequestOwnership();
                });
                manipulator.OnManipulationEnded.AddListener(data =>
                {
                    StartCoroutine(StopMovement());
                    IEnumerator StopMovement()
                    {
                        yield return null;
                        GetComponent<Rigidbody>().velocity = Vector3.zero;
                        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    }
                });
#elif UNITY_ANDROID

#endif
            }
            
            if (Flags.HasFlag(InteractionFlag.Highlight))
            {
                var outline = gameObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineVisible;

                StartCoroutine(nextFrame());
                IEnumerator nextFrame()
                {
                    yield return new WaitForEndOfFrame();
                    outline.enabled = false;
                }
            }
        }

        private GameObject _prevHighlightTarget = null;
        [PunRPC]
        public void Interact(InteractionFlag flag)
        {

            Debug.Log("Interact: "+flag.ToString());
            
            switch (flag)
            {
                case InteractionFlag.Highlight:
                {
                    PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.HIGHLIGHT, photonView.ViewID,
                        RaiseEventOptions.Default, SendOptions.SendReliable);
                    
                    if (_prevHighlightTarget != gameObject)
                    {
                        if (_prevHighlightTarget != null)
                            _prevHighlightTarget.GetComponent<Outline>().enabled = false;
                        _prevHighlightTarget = gameObject;
                    }
                    
                    var outline = gameObject.GetComponent<Outline>();
                    if (!outline)
                    {
                        outline = gameObject.AddComponent<Outline>();
                        outline.OutlineMode = Outline.Mode.OutlineVisible;
                    }
                    outline.enabled = !outline.enabled;
                    
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
                        FindObjectOfType<InteractionManager>().VideoPrefab.name, Collider.bounds.center, transform.rotation,
                        (byte) TargetManager.Groups.PLAYERS, new object[] {photonView.ViewID});
                    video.transform.parent = transform;
                    var scale = video.transform.localScale;
                    scale.y *= (1f * GetComponent<VideoPlayer>().height) / GetComponent<VideoPlayer>().width;
                    transform.localScale = scale;
                    _videoObjects[avatar] = video.GetComponent<OOIInfo>();
                }

                _moveVideo = StartCoroutine(MoveObject(_videoObjects, Quaternion.identity, 
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
                    var playerPosition = playerTransform.position;
                    
                    Vector3 nearestPoint = Collider.ClosestPoint(playerPosition);

                    nearestPoint.y = playerPosition.y;

                    Vector3 newPosition;

                    if (nearestPoint == playerPosition)
                    {
                        newPosition = playerPosition +
                                       Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized *
                                       playerTransform.lossyScale.x;
                    }
                    else
                    {
                        newPosition = playerPosition + (nearestPoint - playerPosition).normalized *
                                       playerTransform.lossyScale.x;
                    }

                    newPosition = newPosition + objTransform.lossyScale.x * positionOffset.y * objTransform.up;
                    objTransform.position = Vector3.Lerp(objTransform.position, newPosition, 0.05f);
                    objTransform.LookAt(new Vector3(playerPosition.x, newPosition.y, playerPosition.z));
                    objTransform.rotation = objTransform.rotation * rotationOffset;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}