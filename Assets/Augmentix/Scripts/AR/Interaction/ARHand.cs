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

    private AbstractInteractable _currentInteractable;
    public AbstractInteractable CurrentInteractable
    {
        set
        {
            Debug.Log("New interactable: "+ (value != null ? value.gameObject.name:"null"));
            if (_currentInteractable != null)
                _currentInteractable.OnInteractionEnd?.Invoke(this);
            
            if (value == null || value.IsBlocked)
                _currentInteractable = null;
            else
                _currentInteractable = value;

            if (_currentInteractable != null)
                _currentInteractable.OnInteractionStart?.Invoke(this);
        }
        get => _currentInteractable;
    }

    public bool IsPinching = false;

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
            Debug.Log("PinchEnd");
            CurrentInteractable = null;
        };
        OnPinchStart += () =>
        {
            Debug.Log("PinchStart");
            if (PinchingSphere.IsColliding() && CurrentInteractable == null)
            {
                CurrentInteractable = PinchingSphere.CurrentInteractable;
            }
        };

        gameObject.SetActive(false);
    }

    public void Update()
    {

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
}
