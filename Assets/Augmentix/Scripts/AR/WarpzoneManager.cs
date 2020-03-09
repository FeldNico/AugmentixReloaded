#if UNITY_WSA
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;


public class WarpzoneManager : MonoBehaviour
{
    public enum IndicatorMode
    {
        None,
        Gazed,
        Selected
    }
    public GameObject WarpzoneGazeIndicator;
    public float ScrollSpeed = 10f;
    public List<Warpzone> Warpzones { get; private set; }
    public UnityAction<Warpzone> OnNewWarpzone;
    public UnityAction<Warpzone> OnWarpzoneLost;

    private Warpzone _activeWarpzone;
    private VirtualCity _virtualCity;
    private Deskzone _deskzone;

    public Warpzone ActiveWarpzone
    {
        get => _activeWarpzone;
        set
        {
            if (_activeWarpzone != null)
            {
                OnWarpzoneLost?.Invoke(_activeWarpzone);
                _activeWarpzone.OnFocusLost?.Invoke();
            }
               
            _activeWarpzone = value;
            if (_activeWarpzone != null)
            {
                OnNewWarpzone?.Invoke(_activeWarpzone);
                _activeWarpzone.OnFocus?.Invoke();
            }
                
        }
    }
    public void Awake()
    {
        Warpzones = FindObjectsOfType<Warpzone>().ToList();
    }

    public void Start()
    {
        _virtualCity = FindObjectOfType<VirtualCity>();
        _deskzone = FindObjectOfType<Deskzone>();
        
        OnWarpzoneLost += Warpzone => { _virtualCity.transform.position = new Vector3(0,500,0); };
    }

    public void Update()
    {
        if (ActiveWarpzone != null)
        {
            _virtualCity.transform.position = _virtualCity.transform.position + (_deskzone.Position - ActiveWarpzone.Position);
        }
    }
}
#endif