
using System;
using Augmentix.Scripts.OOI;

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using Augmentix.Scripts.VR;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;


public class InteractionManager : MonoBehaviour
{
    public GameObject InteractionSpherePrefab;
    public GameObject TextPrefab;
    public GameObject VideoPrefab;
    public GameObject HighlightPrefab;
    public GameObject DeletePrefab;
    public GameObject SpawnableButtonPrefab;
    public Material PointingMaterial;

    public float InteractionSphereScale = 0.03f;
    public float RadianUIRadius = 0.04f;
    
    private Deskzone _deskzone;
    private WarpzoneManager _warpzoneManager;
    private VirtualCity _virtualCity;
    private Camera _mainCamera;
    private LineRenderer _pointer;

    private string[] _axis = new string[] {"Horizontal","Vertical","Fire1","Fire2","Fire3","Jump","Mouse X","Mouse Y","Mouse ScrollWheel","Horizontal","Vertical","Fire1","Fire2","Fire3","Jump","Submit","Submit","Cancel","AXIS_1","AXIS_2","AXIS_3","AXIS_4","AXIS_5","AXIS_6","AXIS_7","AXIS_8","AXIS_9","AXIS_10","AXIS_11","AXIS_12","AXIS_13","AXIS_14","AXIS_15","AXIS_16","AXIS_17","AXIS_18","AXIS_19","AXIS_20","AXIS_21","AXIS_22","AXIS_23","AXIS_24","AXIS_25","AXIS_26","AXIS_27","AXIS_28","UpDown","UpDown","Oculus_GearVR_LThumbstickX","Oculus_GearVR_LThumbstickY","Oculus_GearVR_RThumbstickX","Oculus_GearVR_RThumbstickY","Oculus_GearVR_DpadX","Oculus_GearVR_DpadY","Oculus_GearVR_LIndexTrigger","Oculus_GearVR_RIndexTrigger","Oculus_CrossPlatform_Button2","Oculus_CrossPlatform_Button4","Oculus_CrossPlatform_PrimaryThumbstick","Oculus_CrossPlatform_SecondaryThumbstick","Oculus_CrossPlatform_PrimaryIndexTrigger","Oculus_CrossPlatform_SecondaryIndexTrigger","Oculus_CrossPlatform_PrimaryHandTrigger","Oculus_CrossPlatform_SecondaryHandTrigger","Oculus_CrossPlatform_PrimaryThumbstickHorizontal","Oculus_CrossPlatform_PrimaryThumbstickVertical","Oculus_CrossPlatform_SecondaryThumbstickHorizontal","Oculus_CrossPlatform_SecondaryThumbstickVertical"};
    
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
        /*
        foreach (var key in (KeyCode[]) Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
                Debug.Log(key +" pressed");
        }

        foreach (var axis in _axis)
        {
            if (Math.Abs(Input.GetAxis(axis)) > 0.05f)
                Debug.Log("Axis: "+axis);
        }
        */
        

        //HandleWarpzoneGazing();
        
        if (PhotonNetwork.IsConnected)
        {
            if (_deskzone.IsInside)
            {
                if (_pointer != null && _pointer.enabled)
                {
                    _pointer.enabled = !_pointer.enabled;
                    PhotonNetwork.RaiseEvent( (byte) TargetManager.EventCode.POINTING, null,
                        RaiseEventOptions.Default, SendOptions.SendReliable);
                }
                HandleWarpzoneGazing();
            }
            else
            {
                if (Input.GetKey(KeyCode.Mouse0) && HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, Handedness.Right,out MixedRealityPose pose))
                {
                    var startPos = pose.Position;
                    var endPos = startPos + 50 * pose.Forward;
                    
                    if (_pointer == null)
                    {
                        var go = new GameObject("Pointer");
                        go.transform.parent = transform;
                        go.transform.localPosition = Vector3.zero;
                        _pointer = go.AddComponent<LineRenderer>();
                        _pointer.endWidth = 0.05f;
                        _pointer.startWidth = 0.05f;
                        _pointer.material = PointingMaterial;
                        _pointer.SetPosition(0,startPos);
                        _pointer.SetPosition(1,endPos);
                    }
                    
                    _pointer.SetPosition(0,Vector3.Lerp(_pointer.GetPosition(0),startPos,0.05f));
                    _pointer.SetPosition(1,Vector3.Lerp(_pointer.GetPosition(1),endPos,0.05f));

                    PhotonNetwork.RaiseEvent( (byte) TargetManager.EventCode.POINTING, new object[] {PhotonNetwork.LocalPlayer.ActorNumber, _virtualCity.transform.InverseTransformPoint(startPos), _virtualCity.transform.InverseTransformPoint(endPos)},
                        RaiseEventOptions.Default, SendOptions.SendReliable);
                    
                    _pointer.enabled = true;
                }
                else
                {
                    if (_pointer != null && _pointer.enabled)
                    {
                        _pointer.enabled = !_pointer.enabled;
                        PhotonNetwork.RaiseEvent( (byte) TargetManager.EventCode.POINTING, new object[] {PhotonNetwork.LocalPlayer.ActorNumber},
                            RaiseEventOptions.Default, SendOptions.SendReliable);
                    }
                }
            }
                
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

                if (Input.GetKeyUp(KeyCode.Mouse0) && warpzone != _warpzoneManager.ActiveWarpzone)
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

            var camTransform = _mainCamera.transform;
            
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

                    if (Input.GetAxis("Mouse Y") >0.1f)
                    {
                        vec -= 0.05f* _warpzoneManager.ScrollSpeed * forwardProject;
                    } else if (Input.GetAxis("Mouse Y") < -0.1f)
                    {
                        vec += 0.05f*_warpzoneManager.ScrollSpeed * forwardProject;
                    }
                    
                    if (Input.GetAxis("Mouse X") < -0.1f)
                    {
                        vec += 0.05f* _warpzoneManager.ScrollSpeed * rightProject;
                    } else if (Input.GetAxis("Mouse X") > 0.1f)
                    {
                        vec -= 0.05f*_warpzoneManager.ScrollSpeed * rightProject;
                    }
                    
                    /*
                    if (Input.GetKey(KeyCode.W))
                        vec -= 0.05f* _warpzoneManager.ScrollSpeed * forwardProject;
                    else if (Input.GetKey(KeyCode.X))
                        vec += 0.05f*_warpzoneManager.ScrollSpeed * forwardProject;

                    if (Input.GetKey(KeyCode.A))
                        vec += 0.05f*_warpzoneManager.ScrollSpeed * rightProject;
                    else if (Input.GetKey(KeyCode.D))
                        vec -=0.05f* _warpzoneManager.ScrollSpeed * rightProject;
                        */
                    
                    _warpzoneManager.ActiveWarpzone.LocalPosition += vec;
                }
                
                
            }
            else
            {
                
                var forwardProject = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
                var rightProject = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

                if (Input.GetAxis("Mouse Y") > 0.1f)
                {
                    vec += 0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                } else if (Input.GetAxis("Mouse Y") < -0.1f)
                {
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                }
                    
                if (Input.GetAxis("Mouse X") < -0.1f)
                {
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                } else if (Input.GetAxis("Mouse X") > 0.1f)
                {
                    vec +=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                }
                
                /*
                if (Input.GetKey(KeyCode.W))
                    vec += 0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                else if (Input.GetKey(KeyCode.X))
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;

                if (Input.GetKey(KeyCode.A))
                    vec -=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                else if (Input.GetKey(KeyCode.D))
                    vec +=0.05f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                */
                vec = _virtualCity.transform.InverseTransformVector(vec);
                vec.y = 0;
                
                _warpzoneManager.ActiveWarpzone.LocalPosition += vec;
            }

            
        }
    }
}