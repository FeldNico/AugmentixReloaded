#if UNITY_WSA
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using UnityEngine;


public class WarpzoneManager : MonoBehaviour
{
    public static WarpzoneManager Instance { get; private set; }

    public float ScrollSpeed = 10f;
    public List<Warpzone> Warpzones { get; private set; }

    private Warpzone _activeWarpzone;

    public Warpzone ActiveWarpzone
    {
        get => _activeWarpzone;
        set
        {
            if (_activeWarpzone != null)
                _activeWarpzone.OnFocusLost?.Invoke();
            _activeWarpzone = value;
            if (_activeWarpzone != null)
                _activeWarpzone.OnFocus?.Invoke();
        }
    }
    public void Awake()
    {
        Instance = this;
        Warpzones = FindObjectsOfType<Warpzone>().ToList();
    }
}
#endif