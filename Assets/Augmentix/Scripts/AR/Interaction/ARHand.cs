using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ARHand : MonoBehaviour
{
    public bool IsRight;
    public GameObject Thumb;
    public GameObject Index;
    public GameObject Palm;
    public PinchingSphere PinchingSphere;
    [HideInInspector] public float PinchStrength;
    [HideInInspector] public bool IsDetected = false;
    [HideInInspector] public bool IsPointing = false;
    [HideInInspector] public Vector3 PointingDirection;
    public UnityAction OnDetect;
    public UnityAction OnLost;
    public UnityAction OnPointStart;
    public UnityAction OnPointEnd;
    public UnityAction OnPinchStart;
    public UnityAction OnPinchEnd;
    
    public Interactable GrabbedInteractable { private set; get; }

    private LineRenderer _lineRenderer;
    private Transform _thumbTransform;
    private Transform _indexTransform;
    private Transform _palmTransform;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _thumbTransform = Thumb.transform;
        _indexTransform = Index.transform;
        _palmTransform = Palm.transform;
        
        OnDetect += () =>
        {
            gameObject.SetActive(true);
        };
        OnLost += () =>
        {
            gameObject.SetActive(false);
        };
        OnPinchEnd += () =>
        {
            if (GrabbedInteractable != null)
            {
                GrabbedInteractable.OnPinchEnd?.Invoke(this);
                GrabbedInteractable = null;
            }
        };
        OnPinchStart += () =>
        {
            if (PinchingSphere.IsColliding() && GrabbedInteractable == null)
            {
                GrabbedInteractable = PinchingSphere.CurrentInteractable;
                GrabbedInteractable.OnPinchStart?.Invoke(this);
            }
        };
        OnPointEnd += () => { Debug.Log("OnPointEnd"); };
        OnPointStart += () => { Debug.Log("OnPointStart"); };
    }

    private bool _wasPinching = false;
    public void Update()
    {
        if (IsPinching() ^ _wasPinching) 
        {
            if (_wasPinching)
            {
                _wasPinching = false;
                OnPinchEnd?.Invoke();
            }
            else
            {
                _wasPinching = true;
                OnPinchStart?.Invoke();
            }
        }

        var pinchPos = GetPinchPosition();
        PinchingSphere.transform.position = pinchPos + (pinchPos - _palmTransform.localPosition);

        if (IsPointing)
        {
            if (!_lineRenderer.enabled)
                _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, Index.transform.localPosition);
            _lineRenderer.SetPosition(1, Index.transform.localPosition + PointingDirection * 5);
        }
        else
        {
            if (_lineRenderer.enabled)
                _lineRenderer.enabled = false;
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.zero);
        }
    }


    public bool IsPinching()
    {
        return PinchStrength > 0.8F;
    }
    
    public Vector3 GetPinchPosition() {
        return (2 * Thumb.transform.position + Index.transform.position) * 0.333333F;
    }
}
