using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.AR;
using UnityEngine;

public class MapTargetDummy : MonoBehaviour
{
    public MonoBehaviour Target;

    private InteractionManager _interactionManager;
    private MapTarget _mapTarget;
    private Moveable _interactable;

    void Start()
    {
        _interactionManager = FindObjectOfType<InteractionManager>();
        _mapTarget = FindObjectOfType<MapTarget>();
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "InteractionSphere "+gameObject.name;
        sphere.GetComponent<SphereCollider>().isTrigger = true;
        sphere.AddComponent<Rigidbody>().useGravity = false;
        _interactable = sphere.AddComponent<Moveable>();
        var trans = _interactable.transform;
        trans.localScale =  trans.localScale * _interactionManager.InteractionSphereScale;
        
        if (Target is Warpzone)
        {
            _interactable.OnInteractionKeep += (hand) =>
            {
                var localPos = _mapTarget.Scaler.transform.InverseTransformPoint(_interactable.transform.position);
                localPos.y = 0;
                ((Warpzone) Target).LocalPosition = localPos;
            };
        } else if (Target is PlayerAvatar)
        {
            _interactable.OnInteractionEnd += (hand) =>
            {
                var localPos = _mapTarget.Scaler.transform.InverseTransformPoint(_interactable.transform.position);
                localPos.y = Target.transform.localPosition.y;
            };
        }
    }

    private void Update()
    {
        if (!_interactable.IsInteractedWith && !_interactable.IsBlocked)
            _interactable.transform.position = transform.position + _mapTarget.transform.up * (Target is Warpzone ? 0.07f : 0.035f);
    }

    private void OnDestroy()
    {
        Destroy(_interactable);
    }
}