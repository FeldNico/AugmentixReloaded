
using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.Rendering;
#if UNITY_WSA
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Augmentix.Scripts.AR;
using Augmentix.Scripts.VR;
using Leap.Unity;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour),typeof(DefaultTrackableEventHandler))]
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

    public UnityAction OnFocus;
    public UnityAction OnFocusLost;
    
    private ImageTargetBehaviour _behaviour;
    private DefaultTrackableEventHandler _eventHandler;
    private GameObject _warpzonePositionDummy;
    private Transform _dummyTransform;
    private VirtualCity _virtualCity;
    private Transform _virtualCityTransform;
    private Camera _mainCamera;
    [HideInInspector]
    public ClippingBox ClippingBox = null;

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

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(2f,0,2f);
        collider.isTrigger = true;
        collider.enabled = false;
        
        
        _eventHandler.OnTargetFound.AddListener(() =>
        {
            Debug.Log("Found Trackable");
            doRender = true;
            collider.enabled = true;
        });
        _eventHandler.OnTargetLost.AddListener(() =>
        {
            Debug.Log("Lost Trackable");
            doRender = false;
            collider.enabled = false;
        });
        
        gameObject.layer = LayerMask.NameToLayer("WarpzoneRaycast");
        
        //WarpzoneManager.Instance.ActiveWarpzone = this;

        StartCoroutine(bla());
        IEnumerator bla()
        {
            yield return new WaitForSeconds(0.1f);
            FindObjectOfType<ARTargetManager>().Connect();
        }
    }

    private void LateUpdate()
    {
        _mainCamera.RemoveAllCommandBuffers();
        
        if (doRender && PhotonNetwork.IsConnected)
        {
            var warpzoneMatrix = Matrix4x4.TRS(_dummyTransform.localPosition,
                _dummyTransform.localRotation, _dummyTransform.localScale / Scale).inverse;

            var _commandBuffer = new CommandBuffer();
            foreach (var valueTuple in _virtualCity.RenderList)
            {
                var trans = valueTuple.Item1;
                var mesh = valueTuple.Item3;
                var materials = valueTuple.Item4;

                if ( Vector3.Distance(_dummyTransform.localPosition, _virtualCityTransform.InverseTransformPoint(trans.position)) < 2 * DisplaySize / transform.localScale.x)
                {
                    var m = transform.localToWorldMatrix  * warpzoneMatrix * Matrix4x4.TRS(_virtualCityTransform.InverseTransformPoint(trans.position),
                                _virtualCityTransform.InverseTransformRotation(trans.rotation),
                                trans.lossyScale);

                    for (int i = 0; i < mesh.sharedMesh.subMeshCount; i++)
                    {
                        _commandBuffer.DrawMesh(mesh.sharedMesh,m,materials[i],i,-1);
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
                break;
            }
            case WarpzoneManager.IndicatorMode.Gazed:
            {
                if (_indicator == null)
                {
                    _indicator = Instantiate(WarpzoneManager.Instance.WarpzoneGazeIndicator, transform);
                }
                _indicator.GetComponent<Renderer>().material.color = Color.green;
                break;
            }
            case WarpzoneManager.IndicatorMode.Selected:
            {
                if (_indicator == null)
                {
                    _indicator = Instantiate(WarpzoneManager.Instance.WarpzoneGazeIndicator, transform);
                }
                _indicator.GetComponent<Renderer>().material.color = Color.blue;
                break;
            }
        }
    }
}
#endif