﻿using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MapTargetDummy : MonoBehaviour
{
    public MonoBehaviour Target;

    private InteractionManager _interactionManager;
    private Map _map;
    private LineRenderer _lineRenderer;
    private bool _renderLine;
    private GameObject _sphere;
    private WarpzoneManager _warpzoneManager;

    public bool IsInteractedWith { private set; get; } = false;

    void Start()
    {
        #if UNITY_WSA
        _interactionManager = FindObjectOfType<InteractionManager>();
        _map = FindObjectOfType<Map>();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sphere.transform.localScale = Vector3.one *_interactionManager.InteractionSphereScale;
        _sphere.name = "InteractionOrb " + gameObject.name;
        _sphere.GetComponent<SphereCollider>().isTrigger = true;
        var rigidbody = _sphere.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        
        var manipulator = _sphere.AddComponent<ObjectManipulator>();
        manipulator.ReleaseBehavior = 0;
        manipulator.HostTransform = _sphere.transform;
        manipulator.ManipulationType = ManipulationHandFlags.OneHanded;
        manipulator.OnManipulationStarted.AddListener(eventData => { IsInteractedWith = true; });
        manipulator.OnManipulationEnded.AddListener(eventData =>
        {
            IsInteractedWith = false;
            StartCoroutine(StopMovement());
            IEnumerator StopMovement()
            {
                yield return null;
                _sphere.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _sphere.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        });
        _sphere.AddComponent<NearInteractionGrabbable>();
        var worldConstrain = _sphere.AddComponent<FixedRotationToWorldConstraint>();
        worldConstrain.TargetTransform = _sphere.transform;
        worldConstrain.HandType = ManipulationHandFlags.OneHanded;
        
        if (Target is Warpzone)
        {
            manipulator.OnManipulationStarted.AddListener(eventdata =>
            {
                _warpzoneManager.ActiveWarpzone = (Warpzone) Target;
            });
            manipulator.OnManipulationEnded.AddListener(eventData =>
            {
                var localPos = _map.Scaler.transform.InverseTransformPoint(_sphere.transform.position);
                localPos.y = 0;
                ((Warpzone) Target).LocalPosition = localPos;
            });
        } else if (Target is PlayerAvatar)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.material = _map.LineMaterial;
            _lineRenderer.material.color = Color.blue;
            _lineRenderer.startWidth = 0.01f;
            _lineRenderer.endWidth = 0.01f;
            _renderLine = false;
            _lineRenderer.enabled = _renderLine;
            var map = FindObjectOfType<Map>().GetComponentInParent<DefaultTrackableEventHandler>();
            map.OnTargetFound.AddListener(() =>
            {
                StartCoroutine(UpdateLineRenderer());
                IEnumerator UpdateLineRenderer()
                {
                    yield return null;
                    _lineRenderer.enabled = _renderLine;
                }
            });
            manipulator.OnManipulationStarted.AddListener(eventData =>
            {
                _renderLine = true;
                _lineRenderer.enabled = _renderLine;
                _lineRenderer.material.color = Color.green;
            });
            manipulator.OnManipulationEnded.AddListener(eventData =>
            {
                var localPos = _map.Scaler.transform.InverseTransformPoint(_sphere.transform.position);
                localPos.y = 0;
                _lineRenderer.material.color = Color.blue;
                var options = new RaiseEventOptions();
                options.TargetActors = new[] { Target.GetComponent<PhotonView>().OwnerActorNr };
                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.NAVIGATION,
                    localPos,
                    options, SendOptions.SendReliable);
            });
        }
        #endif
    }

    private void Update()
    {
        if (Target is PlayerAvatar)
        {
            if (IsInteractedWith)
            {
                var localPos = _map.Scaler.transform.InverseTransformPoint(_sphere.transform.position);
                localPos.y = 0;
                _lineRenderer.SetPosition(1, _map.Scaler.transform.TransformPoint(localPos));
            }
            if (_lineRenderer.enabled)
            {
                _lineRenderer.SetPosition(0, transform.position);
                if (!IsInteractedWith && Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)) < 0.02f)
                {
                    _renderLine = false;
                    _lineRenderer.enabled = _renderLine;
                }
            }
        }
        
        if (!IsInteractedWith)
        {
            _sphere.transform.position =
                transform.position + _map.transform.up * (Target is Warpzone ? 0.07f : 0.035f);
        }
    }

    private void OnDestroy()
    {
        Destroy(_sphere);
    }
}