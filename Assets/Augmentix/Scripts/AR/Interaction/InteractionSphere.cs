using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class InteractionSphere : AbstractInteractable
{
    #if UNITY_WSA
    private Transform _ooiTransform;
    private Collider _collider;
    private Vector3 _offset;
    private MeshFilter _mesh;

    private OOI _ooi;
    private WarpzoneManager _warpzoneManager;

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
        OnInteractionStart += (hand) => { _ooi.Interact(OOI.InteractionFlag.Text); };
    }

    private Quaternion _halfXRotation = Quaternion.AngleAxis(180, Vector3.up);
    void Update()
    {
        if (OOI == null || !_collider.enabled)
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