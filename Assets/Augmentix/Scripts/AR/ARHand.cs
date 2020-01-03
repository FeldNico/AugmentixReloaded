using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ARHand : MonoBehaviour
{
    public bool IsRight;
    public GameObject Thumb;
    public GameObject IndexFinger;
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

    #region DEBUG

    private LineRenderer _lineRenderer;

    #endregion
    
    public ARHand()
    {
        OnDetect += () =>
        {
            gameObject.SetActive(true);
        };
        OnLost += () =>
        {
            gameObject.SetActive(false);
        };
        OnPinchEnd += () => { Debug.Log("OnPinchEnd"); };
        OnPinchStart += () => { Debug.Log("OnPinchStart"); };
        OnPointEnd += () => { Debug.Log("OnPointEnd"); };
        OnPointStart += () => { Debug.Log("OnPointStart"); };
    }

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private bool _wasPinching = false;
    public void Update()
    {
        /*
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

        if (IsPointing)
        {
            _lineRenderer.SetPositions(new []{IndexFinger.transform.localPosition, IndexFinger.transform.localPosition + PointingDirection * 5});
        }
        else
        {
            _lineRenderer.SetPositions(new []{Vector3.zero, Vector3.zero});
        }
        */
    }


    public bool IsPinching()
    {
        return PinchStrength > 0.8F;
    }
    
    public Vector3 GetPinchPosition() {
        return (2 * Thumb.transform.position + IndexFinger.transform.position) * 0.333333F;
    }
}
