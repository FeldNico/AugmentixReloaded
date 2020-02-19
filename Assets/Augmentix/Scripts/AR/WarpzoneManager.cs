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

    public void Update()
    {
        if (ActiveWarpzone != null)
        {
            var x = Input.GetAxis("Mouse X");
            var y = Input.GetAxis("Mouse Y");

            if (Math.Abs(x) > 0.5f || Math.Abs(y) > 0.5f)
            {
                var vec = new Vector3();
                if (Math.Abs(x) > 0.5f)
                {
                    vec.x = Math.Sign(x) * ScrollSpeed * ActiveWarpzone.Scale;
                }
                if (Math.Abs(y) > 0.5f)
                {
                    vec.z = Math.Sign(y) * ScrollSpeed * ActiveWarpzone.Scale;
                }

                ActiveWarpzone.Position += vec;
            }
        }
    }
}
#endif