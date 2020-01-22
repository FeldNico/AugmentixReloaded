using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap.Unity;
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ImageTargetBehaviour),typeof(DefaultTrackableEventHandler))]
public class WarpzoneTangible : MonoBehaviour
{
    public Vector3 WarpzonePosition
    {
        set
        {
            if (_warpzonePositionDummy != null)
                _warpzonePositionDummy.transform.localPosition = value;
        }
        get
        {
            if (_warpzonePositionDummy != null)
                return _warpzonePositionDummy.transform.localPosition;

            return Vector3.zero;
        }
    }

    public Vector2 DisplaySize = new Vector2(0.1f,0.1f);
    public float Scale = 0.1f;

    private ImageTargetBehaviour _behaviour;
    private DefaultTrackableEventHandler _eventHandler;
    private GameObject _warpzoneCenterDummy;
    private GameObject _warpzonePositionDummy;
    private VirtualCity _virtualCity;
    private Camera _warpzoneCamera;
    private GameObject _warpzoneCameraDummy;

    private Camera _mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _virtualCity = FindObjectOfType<VirtualCity>();
        _behaviour = GetComponent<ImageTargetBehaviour>();
        _eventHandler = GetComponent<DefaultTrackableEventHandler>();
        
        _warpzoneCenterDummy = new GameObject("Center");
        _warpzoneCenterDummy.transform.parent = transform;
        _warpzoneCenterDummy.transform.localScale = Vector3.one;
        _warpzoneCenterDummy.transform.localPosition = Vector3.zero;
        _warpzoneCenterDummy.transform.localRotation = Quaternion.identity;
        
        _warpzonePositionDummy = new GameObject("TangiblePosition");
        _warpzonePositionDummy.transform.parent = _virtualCity.transform;
        _warpzonePositionDummy.transform.localPosition = new Vector3(0.195f,0,0.055f);
        _warpzonePositionDummy.transform.localScale = Vector3.one;
        _warpzonePositionDummy.transform.localRotation = Quaternion.identity;
        
        _warpzoneCameraDummy = new GameObject("CameraDummy");
        _warpzoneCameraDummy.transform.parent = _warpzonePositionDummy.transform;
        _warpzoneCameraDummy.transform.localPosition = Vector3.zero;
        _warpzoneCameraDummy.transform.localRotation = Quaternion.identity;
        _warpzoneCameraDummy.transform.localScale = Vector3.one;
        
        _warpzoneCamera = new GameObject("Camera").AddComponent<Camera>();
        _warpzoneCamera.transform.parent = _warpzoneCameraDummy.transform;
        _warpzoneCamera.transform.localPosition = Vector3.zero;
        _warpzoneCamera.transform.localRotation = Quaternion.identity;
        _warpzoneCamera.transform.localScale = Vector3.one;
        //_warpzoneCamera.enabled = false;
        _warpzoneCamera.clearFlags = CameraClearFlags.Depth;
        _warpzoneCamera.depth = 10;
        _warpzoneCamera.stereoSeparation = _mainCamera.stereoSeparation;
        _warpzoneCamera.stereoConvergence = _mainCamera.stereoConvergence;
        _warpzoneCamera.stereoTargetEye = StereoTargetEyeMask.Both;

        _eventHandler.OnTargetFound.AddListener(() =>
        {
            Debug.Log("Found Trackable");
            _warpzoneCamera.enabled = true;
            CameraUpdate();
        });
        _eventHandler.OnTargetLost.AddListener(() =>
        {
            Debug.Log("Lost Trackable");
            _warpzoneCamera.enabled = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (_warpzoneCamera.enabled)
        {
            CameraUpdate();
        }
    }

    private void CameraUpdate()
    {
        var trans = _warpzoneCenterDummy.transform;

        var camPos = trans.InverseTransformPoint(_mainCamera.transform.position);
        _warpzoneCameraDummy.transform.localPosition = camPos * Scale;
        _warpzoneCameraDummy.transform.localRotation = trans.InverseTransformRotation(_mainCamera.transform.rotation);
        //var localRotation =  Quaternion.Inverse( trans.rotation) * _mainCamera.transform.rotation;
        //_warpzoneCamera.transform.localRotation = localRotation;
        //_warpzoneCamera.transform.localRotation = Camera.main.transform.rotation;
        _warpzoneCamera.fieldOfView = _mainCamera.fieldOfView;
        _warpzoneCamera.stereoSeparation = _mainCamera.stereoSeparation;
        _warpzoneCamera.stereoConvergence = _mainCamera.stereoConvergence;

        /*
        var pos = trans.localPosition;
        var scale = trans.lossyScale;

        Vector3[] points = new[]
        {
            pos + new Vector3(-DisplaySize.x / scale.x, 0,
                -DisplaySize.y / scale.z),
            pos + new Vector3(-DisplaySize.x / scale.x, 0,
                DisplaySize.y / scale.z),
            pos + new Vector3(DisplaySize.x / scale.x, 0,
                -DisplaySize.y / scale.z),
            pos + new Vector3(DisplaySize.x / scale.x, 0,
                DisplaySize.y / scale.z)
        };

        var screenPoints = points.Select(vector3 =>
        {
            var screenpos = _mainCamera.WorldToScreenPoint(trans.TransformPoint(vector3));
            return new Vector2(screenpos.x / _mainCamera.pixelWidth, screenpos.y / _mainCamera.pixelHeight);
        }).ToArray();
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        foreach (var screenPoint in screenPoints)
        {
            if (screenPoint.x < minX)
                minX = screenPoint.x;
            if (screenPoint.x > maxX)
                maxX = screenPoint.x;
            
            if (screenPoint.y < minY)
                minY = screenPoint.y;
            if (screenPoint.y > maxY)
                maxY = screenPoint.y;
        }
        //_warpzoneCamera.rect = Rect.MinMaxRect(minY,minX,maxY,maxX);
        //_warpzoneCamera.stereoSeparation = _mainCamera.stereoSeparation;
        //_warpzoneCamera.stereoConvergence = _mainCamera.stereoConvergence;
        */
    }
}
