using System;
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
    private GameObject _sphere;
    private WarpzoneManager _warpzoneManager;

    public bool IsInteractedWith { private set; get; } = false;

    void Start()
    {
        _interactionManager = FindObjectOfType<InteractionManager>();
        _map = FindObjectOfType<Map>();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sphere.transform.localScale = Vector3.one *_interactionManager.InteractionSphereScale;
        _sphere.name = "InteractionSphere " + gameObject.name;
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
            _lineRenderer.enabled = false;
            manipulator.OnManipulationStarted.AddListener(eventData =>
            {
                IsInteractedWith = true;
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
    }

    private void Update()
    {
        if (IsInteractedWith && Target is PlayerAvatar)
        {
            _lineRenderer.enabled = true;
            var localPos = _map.Scaler.transform.InverseTransformPoint(_sphere.transform.position);
            localPos.y = 0;
            _lineRenderer.SetPosition(1, _map.Scaler.transform.TransformPoint(localPos));
        }
        
        if (!IsInteractedWith)
        {
            _sphere.transform.position =
                transform.position + _map.transform.up * (Target is Warpzone ? 0.07f : 0.035f);
        }
        
        if (_lineRenderer && _lineRenderer.enabled)
        {
            _lineRenderer.SetPosition(0, transform.position);
            if (Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)) < 0.02f)
                _lineRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(_sphere);
    }
}