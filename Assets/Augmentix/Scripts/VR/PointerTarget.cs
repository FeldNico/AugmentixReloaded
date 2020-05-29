using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using Augmentix.Scripts.VR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PointerTarget : MonoBehaviour
{
    public UnityAction OnHoverStart;
    public UnityAction OnHoverEnd;
    public UnityAction<Vector2> OnPress;
    public UnityAction<Vector2> OnRelease;

    public Vector3 TeleportTarget;

    private OVRPlayerController _player;
    private VirtualCity _virtualCity;
    private Button _button;
    public void Awake()
    {
        _button = GetComponent<Button>();
        _player = FindObjectOfType<OVRPlayerController>();
        _virtualCity = FindObjectOfType<VirtualCity>();
        OnRelease += pos =>
        {
            if (!TeleportTarget.Equals(Vector3.zero))
            {
                StartCoroutine(Teleport());
                IEnumerator Teleport()
                {
                    _player.enabled = false;
                    yield return null;
                    var target = _virtualCity.transform.TransformPoint(TeleportTarget) ;
                    target.y = _player.transform.position.y;
                    _player.transform.position = target;
                    yield return null;
                    _player.enabled = true;
                }
                
                
            }
        };

        if (_button != null)
        {
            OnRelease += pos =>
            {
                var colors = _button.colors;
                var normal = colors.normalColor;
                colors.normalColor = colors.selectedColor;
                colors.selectedColor = normal;
                _button.colors = colors;
            };
            
            OnPress += pos =>
            {
                var colors = _button.colors;
                var normal = colors.normalColor;
                colors.normalColor = colors.selectedColor;
                colors.selectedColor = normal;
                _button.colors = colors;
            };

            OnHoverStart += () =>
            {
                var colors = _button.colors;
                var normal = colors.normalColor;
                colors.normalColor = colors.highlightedColor;
                colors.highlightedColor = normal;
                _button.colors = colors;
            };
        
            OnHoverEnd += () =>
            {
                var colors = _button.colors;
                var normal = colors.normalColor;
                colors.normalColor = colors.highlightedColor;
                colors.highlightedColor = normal;
                _button.colors = colors;
            };
        }
    }
}
