
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
    public GameObject InteractionOrbPrefab;
    public GameObject TextPrefab;
    public GameObject VideoPrefab;
    public GameObject HighlightPrefab;
    public GameObject DeletePrefab;
    public Material PointingMaterial;

    public float InteractionSphereScale = 0.03f;
    public float RadianUIRadius = 0.04f;
    
    private Deskzone _deskzone;
    private WarpzoneManager _warpzoneManager;
    private VirtualCity _virtualCity;
    private Camera _mainCamera;
    private LineRenderer _pointer;
    
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
                if (!_deskzone.IsInside && Input.GetKey(KeyCode.Mouse0) && HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, Handedness.Right,out MixedRealityPose pose))
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

                    _warpzoneManager.ActiveWarpzone.LocalPosition += vec;
                }
                
                
            }
            else
            {
                
                var forwardProject = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
                var rightProject = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

                if (Input.GetAxis("Mouse Y") > 0.1f)
                {
                    vec += 0.02f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                } else if (Input.GetAxis("Mouse Y") < -0.1f)
                {
                    vec -=0.02f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * forwardProject;
                }
                    
                if (Input.GetAxis("Mouse X") < -0.1f)
                {
                    vec -=0.02f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                } else if (Input.GetAxis("Mouse X") > 0.1f)
                {
                    vec +=0.02f * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * rightProject;
                }
                
                vec = _virtualCity.transform.InverseTransformVector(vec);
                vec.y = 0;
                
                _warpzoneManager.ActiveWarpzone.LocalPosition += vec;
            }

            
        }
    }
}