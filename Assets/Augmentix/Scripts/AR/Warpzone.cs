﻿
using UnityEngine.Rendering;
#if UNITY_WSA
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour),typeof(DefaultTrackableEventHandler))]
public class Warpzone : MonoBehaviour
{
    public Vector3 Position
    {
        set => _dummyTransform.localPosition = value;
        get => _dummyTransform.localPosition;
    }

    public float DisplaySize = 3f;
    public float Scale = 0.05f;

    public UnityAction OnFocus;
    public UnityAction OnFocusLost;
    
    private ImageTargetBehaviour _behaviour;
    private DefaultTrackableEventHandler _eventHandler;
    private GameObject _warpzonePositionDummy;
    private Transform _dummyTransform;
    private VirtualCity _virtualCity;
    private Transform _virtualCityTransform;
    private Camera _mainCamera;
    private GameObject[] _clippingPlanes = new GameObject[4];

    public bool doRender = false;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _behaviour = GetComponent<ImageTargetBehaviour>();
        _eventHandler = GetComponent<DefaultTrackableEventHandler>();
        _virtualCity = FindObjectOfType<VirtualCity>();
        _virtualCityTransform = _virtualCity.transform;
        
        _warpzonePositionDummy = new GameObject("TangiblePosition");
        _dummyTransform = _warpzonePositionDummy.transform;
        _dummyTransform.parent = _virtualCityTransform;
        _dummyTransform.localPosition = new Vector3(46.2f,0,374.1f);
        _dummyTransform.localScale = Vector3.one;
        _dummyTransform.localRotation = Quaternion.identity;

        _eventHandler.OnTargetFound.AddListener(() =>
        {
            Debug.Log("Found Trackable");
            doRender = true;
        });
        _eventHandler.OnTargetLost.AddListener(() =>
        {
            Debug.Log("Lost Trackable");
            doRender = false;
        });

        for (int i = 0; i < _clippingPlanes.Length; i++)
        {
            _clippingPlanes[i] = Instantiate(WarpzoneManager.Instance.ClippingPlanePrefab);
            _clippingPlanes[i].transform.parent = transform;
            _clippingPlanes[i].transform.localScale = new Vector3(5,5,5);
            var vec = new Vector3();
            switch (i)
            {
                case 0:
                {
                    vec.x = -DisplaySize / transform.localScale.x;
                    vec.y = -DisplaySize / transform.localScale.x;
                    break;
                }
                case 1:
                {
                    vec.x = DisplaySize / transform.localScale.x;
                    vec.y = -DisplaySize / transform.localScale.x;
                    break;
                }
                case 2:
                {
                    vec.x = -DisplaySize / transform.localScale.x;
                    vec.y = DisplaySize / transform.localScale.x;
                    break;
                }
                case 3:
                {
                    vec.x = DisplaySize / transform.localScale.x;
                    vec.y = DisplaySize / transform.localScale.x;
                    break;
                }
            }

            _clippingPlanes[i].transform.localPosition = vec;
            _clippingPlanes[i].transform.LookAt(transform);
            _clippingPlanes[i].transform.localRotation =
            _clippingPlanes[i].transform.localRotation * Quaternion.Euler(90, 0, 0);
        }
        
        FindObjectOfType<WarpzoneManager>().ActiveWarpzone = this;
    }

    private void LateUpdate()
    {
        _mainCamera.RemoveAllCommandBuffers();
        
        if (doRender)
        {
            var warpzoneMatrix = Matrix4x4.TRS(_dummyTransform.localPosition,
                _dummyTransform.localRotation, _dummyTransform.localScale / Scale).inverse;
            
            var _commandBuffer = new CommandBuffer();
            foreach (var valueTuple in _virtualCity.RenderList)
            {
                var trans = valueTuple.Item1;
                var renderer = valueTuple.Item2;
                var mesh = valueTuple.Item3;

                if (Vector3.Distance(_dummyTransform.localPosition, _virtualCityTransform.InverseTransformPoint(trans.position)) < 2 * DisplaySize / transform.localScale.x)
                {
                    var m = transform.localToWorldMatrix  * warpzoneMatrix * Matrix4x4.TRS(_virtualCityTransform.InverseTransformPoint(trans.position),
                                _virtualCityTransform.InverseTransformRotation(trans.rotation),
                                trans.lossyScale);

                    for (int i = 0; i < mesh.sharedMesh.subMeshCount; i++)
                        _commandBuffer.DrawMesh(mesh.sharedMesh,m,renderer.materials[i],i,-1);
                }
            }
            _mainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
        }
    }
}
#endif