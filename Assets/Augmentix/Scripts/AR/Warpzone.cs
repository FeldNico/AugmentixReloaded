using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Augmentix.Scripts.AR;
using Augmentix.Scripts.VR;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_WSA
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour),typeof(DefaultTrackableEventHandler))]
#endif
public class Warpzone : MonoBehaviour
{
    public Vector3 LocalPosition
    {
        set => _dummyTransform.localPosition = value;
        get => _dummyTransform.localPosition;
    }

    public Vector3 Position
    {
        set => _dummyTransform.position = value;
        get => _dummyTransform.position;
    }

    public float DisplaySize = 3f;
    public float Scale = 0.05f;
    public Dictionary<Transform, Matrix4x4> Matrices = new Dictionary<Transform, Matrix4x4>();

    public UnityAction OnFocus;
    public UnityAction OnFocusLost;

#if UNITY_WSA
    private ImageTargetBehaviour _behaviour;
    private DefaultTrackableEventHandler _eventHandler;
#endif
    private GameObject _warpzonePositionDummy;
    private Transform _dummyTransform;
    private VirtualCity _virtualCity;
    private Transform _virtualCityTransform;
    private Camera _mainCamera;
    private WarpzoneManager _warpzoneManager;

    public bool IsRendering { private set; get; } = false;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
#if UNITY_WSA
        _behaviour = GetComponent<ImageTargetBehaviour>();
        _eventHandler = GetComponent<DefaultTrackableEventHandler>();
#endif
        _virtualCity = FindObjectOfType<VirtualCity>();
        _virtualCityTransform = _virtualCity.transform;
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();

        _warpzonePositionDummy = new GameObject("TangiblePosition");
        _dummyTransform = _warpzonePositionDummy.transform;
        _dummyTransform.parent = _virtualCityTransform;
        _dummyTransform.localPosition = new Vector3(46.2f, 0, 374.1f);
        _dummyTransform.localScale = Vector3.one;
        _dummyTransform.localRotation = Quaternion.identity;

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.2f, 0.01f, 0.2f);
        collider.isTrigger = true;
        collider.enabled = false;

#if UNITY_WSA
        _eventHandler.OnTargetFound.AddListener(() =>
        {
            Debug.Log("Found Trackable");
            IsRendering = true;
            collider.enabled = true;
        });
        _eventHandler.OnTargetLost.AddListener(() =>
        {
            Debug.Log("Lost Trackable");
            IsRendering = false;
            collider.enabled = false;
        });
#endif

        gameObject.layer = LayerMask.NameToLayer("WarpzoneRaycast");
    }

    private void LateUpdate()
    {
        _mainCamera.RemoveAllCommandBuffers();

        if (IsRendering && PhotonNetwork.IsConnected)
        {

            var warpzoneMatrix = Matrix4x4.TRS(_dummyTransform.localPosition,
                _dummyTransform.localRotation, _dummyTransform.localScale / Scale).inverse;

            var _commandBuffer = new CommandBuffer();
            Matrices.Clear();
            foreach (var valueTuple in _virtualCity.RenderList)
            {
                var renderer = valueTuple.Item2;
                var trans = valueTuple.Item1;
                var mesh = valueTuple.Item3;
                var materials = valueTuple.Item4;
                var scale = valueTuple.Item5;
                
                if (renderer == null)
                    continue;

                if (Vector3.Distance(_dummyTransform.localPosition,
                        _virtualCityTransform.InverseTransformPoint(trans.position)) <
                    2 * DisplaySize / transform.localScale.x)
                {
                    var ooi = trans.GetComponent<OOI>();
                    if (ooi)
                    {
                        ooi.InteractionOrb.GetComponent<Renderer>().enabled = true;
                        ooi.InteractionOrb.GetComponent<Collider>().enabled = true;
                    }
                    
                    Matrices[trans] = transform.localToWorldMatrix * warpzoneMatrix * Matrix4x4.TRS(
                                          _virtualCityTransform.InverseTransformPoint(trans.position),
                                          Quaternion.Inverse(_virtualCityTransform.rotation) * trans.rotation,
                                          trans.lossyScale * scale);

                    for (int i = 0; i < mesh.sharedMesh.subMeshCount; i++)
                    {
                        _commandBuffer.DrawMesh(mesh.sharedMesh, Matrices[trans], materials[i], i, -1);
                    }
                }
                else
                {
                    var ooi = trans.GetComponent<OOI>();
                    if (ooi && !ooi.IsBeingManipulated)
                    {
                        ooi.InteractionOrb.GetComponent<Renderer>().enabled = false;
                        ooi.InteractionOrb.GetComponent<Collider>().enabled = false;
                    }
                }
            }

            _mainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
        }
    }

    private GameObject _indicator;

    public void SetIndicationMode(WarpzoneManager.IndicatorMode mode)
    {
        switch (mode)
        {
            case WarpzoneManager.IndicatorMode.None:
            {
                if (_indicator)
                    Destroy(_indicator);

                _indicator = null;
                
                var dummy = FindObjectsOfType<MapTargetDummy>().First(target => target.Target == this);
                if (dummy != null)
                    dummy.GetComponentInChildren<Renderer>().material.color = Color.blue;
                
                break;
            }
            case WarpzoneManager.IndicatorMode.Gazed:
            {
                if (_indicator == null)
                    _indicator = Instantiate(_warpzoneManager.WarpzoneGazeIndicator, transform);

                var dummy = FindObjectsOfType<MapTargetDummy>().First(target => target.Target == this);
                if (dummy != null)
                    dummy.GetComponentInChildren<Renderer>().material.color = Color.blue;

                _indicator.GetComponent<Renderer>().material.color = Color.blue;
                break;
            }
            case WarpzoneManager.IndicatorMode.Selected:
            {
                if (_indicator == null)
                    _indicator = Instantiate(_warpzoneManager.WarpzoneGazeIndicator, transform);

                var dummy = FindObjectsOfType<MapTargetDummy>().First(target => target.Target == this);
                if (dummy != null)
                    dummy.GetComponentInChildren<Renderer>().material.color = Color.green;
                
                _indicator.GetComponent<Renderer>().material.color = Color.green;
                break;
            }
        }
    }
}