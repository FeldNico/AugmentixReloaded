#if UNITY_WSA
using System;
using Augmentix.Scripts.OOI;

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Augmentix.Scripts.VR;
using Photon.Pun;
using TMPro;
using UnityEngine;

using Vuforia;


public class InteractionManager : MonoBehaviour
{
    public GameObject InteractionSpherePrefab;
    public GameObject TextPrefab;
    public GameObject VideoPrefab;
    public GameObject HighlightPrefab;
    
    private Deskzone _deskzone;
    private WarpzoneManager _warpzoneManager;
    private VirtualCity _virtualCity;
    private Camera _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        _deskzone = FindObjectOfType<Deskzone>();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _virtualCity = FindObjectOfType<VirtualCity>();
        _mainCamera = Camera.main;
    }


    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            HandleWarpzoneGazing();
            HandleWarpzoneMoving();
        }
    }


    private Warpzone _currentGazedWarpzone = null;

    private void HandleWarpzoneGazing()
    {
        var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0), Camera.MonoOrStereoscopicEye.Mono);
        if (Physics.Raycast(ray, out var hit, 5, LayerMask.GetMask("WarpzoneRaycast")))
        {
            var warpzone = hit.collider.GetComponent<Warpzone>();
            if (warpzone != null)
            {
                if (_currentGazedWarpzone != warpzone)
                {
                    if (_currentGazedWarpzone != null)
                        _currentGazedWarpzone.SetIndicationMode(WarpzoneManager.IndicatorMode.None);

                    if (_warpzoneManager.ActiveWarpzone != warpzone)
                    {
                        _currentGazedWarpzone = warpzone;
                        _currentGazedWarpzone.SetIndicationMode(WarpzoneManager.IndicatorMode.Gazed);
                    }
                }

                if (Input.GetKeyUp(KeyCode.U) && warpzone != _warpzoneManager.ActiveWarpzone)
                {
                    if (_warpzoneManager.ActiveWarpzone != null && warpzone != _warpzoneManager.ActiveWarpzone)
                        _warpzoneManager.ActiveWarpzone.SetIndicationMode(WarpzoneManager.IndicatorMode.None);

                    _warpzoneManager.ActiveWarpzone = warpzone;
                    _warpzoneManager.ActiveWarpzone.SetIndicationMode(WarpzoneManager.IndicatorMode.Selected);
                }
            }
        }
        else
        {
            if (_currentGazedWarpzone != null)
            {
                if (_currentGazedWarpzone != _warpzoneManager.ActiveWarpzone)
                    _currentGazedWarpzone.SetIndicationMode(WarpzoneManager.IndicatorMode.None);

                _currentGazedWarpzone = null;
            }
        }
    }


    /*
 *     Up: W
 *     Down: X
 *     Left: A
 *     Right: D
 *     A: U
 *     B: H
 */
    private void HandleWarpzoneMoving()
    {
        var activeWarpzone = _warpzoneManager.ActiveWarpzone;

        if (activeWarpzone != null)
        {
            var vec = new Vector3();

            var camTransform = Camera.main.transform;
            
            if (_deskzone.IsInside)
            {
                if (Input.GetKey(KeyCode.H))
                {
                    if (Input.GetKey(KeyCode.W) && activeWarpzone.Scale < 0.5f)
                        activeWarpzone.Scale += 0.01f;
                    else if (Input.GetKey(KeyCode.X) && activeWarpzone.Scale > 0.02f)
                        activeWarpzone.Scale -= 0.01f;
                }
                else
                {
                    var forwardProject = activeWarpzone.transform.InverseTransformVector(camTransform.forward);
                    var rightProject = activeWarpzone.transform.InverseTransformVector(camTransform.right);
                    forwardProject = new Vector3(forwardProject.x,0,forwardProject.z).normalized;
                    rightProject = new Vector3(rightProject.x,0,rightProject.z).normalized;

                    if (Input.GetKey(KeyCode.W))
                        vec -= 0.05f* _warpzoneManager.ScrollSpeed * forwardProject;
                    else if (Input.GetKey(KeyCode.X))
                        vec += 0.05f*_warpzoneManager.ScrollSpeed * forwardProject;

                    if (Input.GetKey(KeyCode.A))
                        vec += 0.05f*_warpzoneManager.ScrollSpeed * rightProject;
                    else if (Input.GetKey(KeyCode.D))
                        vec -=0.05f* _warpzoneManager.ScrollSpeed * rightProject;
                    
                    _warpzoneManager.ActiveWarpzone.LocalPosition += vec;
                }
                
                
            }
            else
            {
                
                var forwardProject = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
                var rightProject = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

                if (Input.GetKey(KeyCode.W))
                    vec += 0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                else if (Input.GetKey(KeyCode.X))
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;

                if (Input.GetKey(KeyCode.A))
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                else if (Input.GetKey(KeyCode.D))
                    vec +=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                
                _warpzoneManager.ActiveWarpzone.Position += vec;
            }

            
        }
    }
}
#endif