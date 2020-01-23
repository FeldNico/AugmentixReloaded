
using UnityEngine.Rendering;
#if UNITY_WSA
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    public float DisplaySize = 0.3f;
    public float Scale = 0.1f;

    private ImageTargetBehaviour _behaviour;
    private DefaultTrackableEventHandler _eventHandler;
    private GameObject _warpzonePositionDummy;
    private VirtualCity _virtualCity;
    private Camera _mainCamera;

    public bool doRender = false;
    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _virtualCity = FindObjectOfType<VirtualCity>();
        _behaviour = GetComponent<ImageTargetBehaviour>();
        _eventHandler = GetComponent<DefaultTrackableEventHandler>();
        
        _warpzonePositionDummy = new GameObject("TangiblePosition");
        _warpzonePositionDummy.transform.parent = _virtualCity.transform;
        _warpzonePositionDummy.transform.localPosition = new Vector3(46.2f,0,374.1f);
        _warpzonePositionDummy.transform.localScale = Vector3.one;
        _warpzonePositionDummy.transform.localRotation = Quaternion.identity;

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
    }

    
    private void LateUpdate()
    {
        _mainCamera.RemoveAllCommandBuffers();
        
        if (doRender)
        {
            var citytransform = _virtualCity.transform;
            var centerPos = _warpzonePositionDummy.transform.position;
            
            var warpzoneTrans = _warpzonePositionDummy.transform;
            var warpzoneMatrix = Matrix4x4.TRS(warpzoneTrans.localPosition,
                warpzoneTrans.localRotation, warpzoneTrans.localScale / Scale).inverse;
        
            
            var _commandBuffer = new CommandBuffer();
            foreach (var valueTuple in _virtualCity.RenderList)
            {
                var trans = valueTuple.Item1;
                var renderer = valueTuple.Item2;
                var mesh = valueTuple.Item3;

                if (Vector3.Distance(warpzoneTrans.localPosition, citytransform.InverseTransformPoint(trans.position)) < DisplaySize / transform.localScale.x)
                {
                    var m = transform.localToWorldMatrix  * warpzoneMatrix * Matrix4x4.TRS(citytransform.InverseTransformPoint(trans.position),
                                citytransform.InverseTransformRotation(trans.rotation),
                                trans.lossyScale);

                    for (int i = 0; i < mesh.sharedMesh.subMeshCount; i++)
                    {
                        //_commandBuffer.DrawMeshInstanced(mesh.sharedMesh,i,renderer.materials[i],-1,new []{m});
                        _commandBuffer.DrawMesh(mesh.sharedMesh,m,renderer.materials[i],i,-1);
                    }
                }
            }
            _mainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
        }
    }

    private void CameraUpdate()
    {
        /*
        var trans = _warpzoneCenterDummy.transform;

        var camPos = trans.InverseTransformPoint(_mainCamera.transform.position);
        _warpzoneCameraDummy.transform.localPosition = camPos * Scale;
        _warpzoneCameraDummy.transform.localRotation = trans.InverseTransformRotation(_mainCamera.transform.rotation);

        _warpzoneCamera.fieldOfView = _mainCamera.fieldOfView;
        _warpzoneCamera.stereoSeparation = _mainCamera.stereoSeparation;
        _warpzoneCamera.stereoConvergence = _mainCamera.stereoConvergence;
        
        Matrix4x4 projectionMatrix = new Matrix4x4()
        {
            m00 = (2 * camPos.y) / (-DisplaySize.y - DisplaySize.y), m01 = 0, m02 = 0, m03 = 0,
            m10 = 0, m11 = (2 * camPos.y) / (DisplaySize.x + DisplaySize.x), m12 = 0, m13 = 0,
            m20 = (-2 * (camPos.x - (-DisplaySize.y + DisplaySize.y) / 2)) / (-DisplaySize.y - DisplaySize.y),
            m21 = (-2 * (camPos.z - (DisplaySize.x - DisplaySize.x) / 2)) / (DisplaySize.x + DisplaySize.x),
            m22 = (2 * camPos.y - _mainCamera.farClipPlane - _mainCamera.nearClipPlane) /
                  (_mainCamera.farClipPlane - _mainCamera.nearClipPlane),
            m23 = -1,
            m30 = -((-DisplaySize.y + DisplaySize.y) / (-DisplaySize.y - DisplaySize.y)) * camPos.y,
            m31 = -((-DisplaySize.x + DisplaySize.x) / (-DisplaySize.x - DisplaySize.x)) * camPos.y,
            m32 = (-camPos.y * (2 * camPos.y - _mainCamera.farClipPlane - _mainCamera.nearClipPlane) +
                   2 * (_mainCamera.farClipPlane - camPos.y) * (_mainCamera.nearClipPlane - camPos.z)) /
                  (_mainCamera.farClipPlane - _mainCamera.nearClipPlane),
            m33 = camPos.y
        };
        _warpzoneCamera.projectionMatrix = projectionMatrix;
        */
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
#endif