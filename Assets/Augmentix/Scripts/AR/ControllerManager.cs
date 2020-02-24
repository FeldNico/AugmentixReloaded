using System;
using Augmentix.Scripts.OOI;
#if UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Augmentix.Scripts.VR;
using Photon.Pun;
using UnityEngine;
using Vuforia;

public class ControllerManager : MonoBehaviour
{
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

    private string[] axisList = new[]
    {
        "Horizontal", "Vertical", "Fire1", "Fire2", "Fire3", "Jump", "Mouse X", "Mouse Y", "Mouse ScrollWheel",
        "Horizontal", "Vertical", "Fire1", "Fire2", "Fire3", "Jump", "Submit", "Submit", "Cancel", "AXIS_1",
        "AXIS_2", "AXIS_3", "AXIS_4", "AXIS_5", "AXIS_6", "AXIS_7", "AXIS_8", "AXIS_9", "AXIS_10", "AXIS_11",
        "AXIS_12", "AXIS_13", "AXIS_14", "AXIS_15", "AXIS_16", "AXIS_17", "AXIS_18", "AXIS_19", "AXIS_20",
        "AXIS_21", "AXIS_22", "AXIS_23", "AXIS_24", "AXIS_25", "AXIS_26", "AXIS_27", "AXIS_28", "UpDown", "UpDown",
        "Oculus_GearVR_LThumbstickX", "Oculus_GearVR_LThumbstickY", "Oculus_GearVR_RThumbstickX",
        "Oculus_GearVR_RThumbstickY", "Oculus_GearVR_DpadX", "Oculus_GearVR_DpadY", "Oculus_GearVR_LIndexTrigger",
        "Oculus_GearVR_RIndexTrigger", "Oculus_CrossPlatform_Button2", "Oculus_CrossPlatform_Button4",
        "Oculus_CrossPlatform_PrimaryThumbstick", "Oculus_CrossPlatform_SecondaryThumbstick",
        "Oculus_CrossPlatform_PrimaryIndexTrigger", "Oculus_CrossPlatform_SecondaryIndexTrigger",
        "Oculus_CrossPlatform_PrimaryHandTrigger", "Oculus_CrossPlatform_SecondaryHandTrigger",
        "Oculus_CrossPlatform_PrimaryThumbstickHorizontal", "Oculus_CrossPlatform_PrimaryThumbstickVertical",
        "Oculus_CrossPlatform_SecondaryThumbstickHorizontal", "Oculus_CrossPlatform_SecondaryThumbstickVertical"
    };


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
                if (_currentGazedWarpzone != null && _currentGazedWarpzone != _warpzoneManager.ActiveWarpzone)
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
            var forwardProject = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
            var rightProject = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;
            
            if (_deskzone.IsInside)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    vec += _warpzoneManager.ScrollSpeed * activeWarpzone.Scale * forwardProject;
                }
                else if (Input.GetKey(KeyCode.X))
                {
                    vec -= _warpzoneManager.ScrollSpeed * activeWarpzone.Scale * forwardProject;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    vec += _warpzoneManager.ScrollSpeed * activeWarpzone.Scale * rightProject;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    vec -= _warpzoneManager.ScrollSpeed * activeWarpzone.Scale * rightProject;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    vec += 0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                }
                else if (Input.GetKey(KeyCode.X))
                {
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    vec +=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                }
            }

            _warpzoneManager.ActiveWarpzone.Position += vec;
        }
    }
}
#endif