using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MapTargetDummy : MonoBehaviour
{
    public MonoBehaviour Target;

    private InteractionManager _interactionManager;
    private MapTarget _mapTarget;
    private Moveable _interactable;
    private LineRenderer _lineRenderer;

    void Start()
    {
        _interactionManager = FindObjectOfType<InteractionManager>();
        _mapTarget = FindObjectOfType<MapTarget>();
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "InteractionSphere " + gameObject.name;
        sphere.GetComponent<SphereCollider>().isTrigger = true;
        sphere.AddComponent<Rigidbody>().useGravity = false;
        _interactable = sphere.AddComponent<Moveable>();
        var trans = _interactable.transform;
        trans.localScale = trans.localScale * _interactionManager.InteractionSphereScale;

        if (Target is Warpzone)
        {
            _interactable.OnInteractionKeep += (hand) =>
            {
                var localPos = _mapTarget.Scaler.transform.InverseTransformPoint(_interactable.transform.position);
                localPos.y = 0;
                ((Warpzone) Target).LocalPosition = localPos;
            };
        }
        else if (Target is PlayerAvatar)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.startColor = Color.blue;
            _lineRenderer.endColor = Color.blue;
            _lineRenderer.enabled = false;
            
            _interactable.OnInteractionEnd += (hand) =>
            {
                var localPos = _mapTarget.Scaler.transform.InverseTransformPoint(_interactable.transform.position);
                localPos.y = 0;
                PhotonNetwork.RaiseEvent((byte) TargetManager.EventCode.NAVIGATION,
                    localPos,
                    RaiseEventOptions.Default, SendOptions.SendReliable);

                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(1,_mapTarget.Scaler.transform.TransformPoint(localPos));
            };
        }
    }

    private void Update()
    {
        if (!_interactable.IsInteractedWith && !_interactable.IsBlocked)
            _interactable.transform.position =
                transform.position + _mapTarget.transform.up * (Target is Warpzone ? 0.07f : 0.035f);

        if (_lineRenderer && _lineRenderer.enabled)
        {
            _lineRenderer.SetPosition(0,transform.position);
            if (Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)) < 0.02f)
                _lineRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(_interactable);
    }
}