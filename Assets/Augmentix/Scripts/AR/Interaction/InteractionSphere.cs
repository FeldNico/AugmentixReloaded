using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using Leap;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class InteractionSphere : AbstractInteractable
{

    private Transform _ooiTransform;
    private Collider _collider;
    private Vector3 _offset;

    private OOI _ooi;
    private WarpzoneManager _warpzoneManager;

    public OOI OOI
    {
        set
        {
            _ooi = value;
            _ooiTransform = _ooi.transform;
            _collider = _ooi.GetComponent<Collider>();
            //transform.parent = _ooiTransform;
        }
        get => _ooi;
    }

    new void Start()
    {
        base.Start();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        OnInteractionStart += (hand) => { };
    }

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
                    var center = _ooiTransform.TransformVector(_ooi.GetComponent<MeshFilter>().mesh.bounds.center)*warpzone.Scale + new Vector3(m[0, 3], m[1, 3], m[2, 3]);
                    transform.position = center +  (new Vector3(center.x, _collider.ClosestPoint(new Vector3(center.x,float.MaxValue,center.z)).y + 0.07f,center.z)- center);
                }
                transform.localScale = Vector3.one * (0.3f / _warpzoneManager.ActiveWarpzone.Scale);
            }
        }
        else
        {
            transform.localScale = new Vector3(0.03f / transform.lossyScale.x,0.03f / transform.lossyScale.y,0.03f / transform.lossyScale.z);
            var center = _collider.bounds.center;
            transform.position = center +  (new Vector3(center.x, _collider.ClosestPoint(new Vector3(center.x,float.MaxValue,center.z)).y + 0.07f,center.z)- center);
        }

       
    }
}
