using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class InteractionSphere : AbstractInteractable
{
    #if UNITY_WSA
    
    public List<GameObject> MenuItems { private set; get; } = new List<GameObject>();
    
    private Transform _ooiTransform;
    private Collider _collider;
    private Vector3 _offset;
    private MeshFilter _mesh;

    private OOI _ooi;
    private WarpzoneManager _warpzoneManager;
    private InteractionManager _interactionManager;

    public OOI OOI
    {
        set
        {
            _ooi = value;
            _ooiTransform = _ooi.transform;
            _collider = _ooi.GetComponent<Collider>();
            _mesh = _ooi.GetComponent<MeshFilter>();
        }
        get => _ooi;
    }

    new void Start()
    {
        base.Start();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _interactionManager = FindObjectOfType<InteractionManager>();
        var go = new GameObject();
        go.transform.position = transform.position;

        var interactionList = new List<OOI.InteractionFlag>();
        if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Text))
        {
            interactionList.Add(OOI.InteractionFlag.Text);
        }
        if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Video))
        {
            interactionList.Add(OOI.InteractionFlag.Video);
        }
        if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Highlight))
        {
            interactionList.Add(OOI.InteractionFlag.Highlight);
        }
        
        OnInteractionStart += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint == null)
                joint = gameObject.AddComponent<FixedJoint>();

            joint.connectedBody = hand.PinchingSphere.GetComponent<Rigidbody>();
            
            foreach (var flag in interactionList)
            {
                GameObject item = null;
                switch (flag)
                {
                    case OOI.InteractionFlag.Text:
                    {
                        item = Instantiate(_interactionManager.TextPrefab,transform.position,transform.rotation, transform);
                        break;
                    }
                    case OOI.InteractionFlag.Video:
                    {
                        item = Instantiate(_interactionManager.VideoPrefab,transform.position,transform.rotation, transform);
                        break;
                    }
                    case OOI.InteractionFlag.Highlight:
                    {
                        item = Instantiate(_interactionManager.HighlightPrefab,transform.position,transform.rotation, transform);
                        break;
                    }
                }
                MenuItems.Add(item);
            }
        };

        OnInteractionEnd += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint != null && joint.connectedBody == hand.PinchingSphere.GetComponent<Rigidbody>())
                Destroy(joint);
            
            
        };
        
    }

    private Quaternion _halfXRotation = Quaternion.AngleAxis(0, Vector3.up);
    void Update()
    {
        if (OOI == null || !_collider.enabled || IsInteractedWith)
            return;

        if (_ooi.StaticOOI)
        {
            if (_warpzoneManager.ActiveWarpzone != null)
            {
                var warpzone = _warpzoneManager.ActiveWarpzone;
                if (warpzone.Matrices.ContainsKey(_ooiTransform))
                {
                    var m = warpzone.Matrices[_ooiTransform];
                    var bounds = _mesh.mesh.bounds;
                    var position = new Vector3(m[0, 3], m[1, 3], m[2, 3]);
                    var scale = m.GetColumn(0).magnitude;
                    var rotation = Quaternion.LookRotation(
                        m.GetColumn(2),
                        m.GetColumn(1)
                    );
                    var center = rotation * _ooiTransform.TransformVector( _halfXRotation * bounds.center) * scale + position;
                    center.y += (_collider.ClosestPoint(Quaternion.Inverse(rotation) * new Vector3(bounds.center.x, float.MaxValue, bounds.center.z)).y + 0.1f) * scale;
                    transform.position = center;
                }

                transform.localScale = Vector3.one * 0.03f;
            }
        }
        else
        {
            transform.localScale = new Vector3(0.03f / transform.lossyScale.x, 0.03f / transform.lossyScale.y,
                0.03f / transform.lossyScale.z);
            var center = _collider.bounds.center;
            transform.position = center + (new Vector3(center.x,
                                               _collider.ClosestPoint(new Vector3(center.x, float.MaxValue, center.z))
                                                   .y + 0.07f, center.z) - center);
        }
    }
#endif
}