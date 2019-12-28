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
    public float PinchStrength;
    public bool IsDetected {private set; get; } = false;
    public UnityAction OnDetect;
    public UnityAction OnLost;

    public ARHand()
    {
        OnDetect += () =>
        {
            gameObject.SetActive(true);
            IsDetected = true;
        };
        OnLost += () =>
        {
            IsDetected = false;
            gameObject.SetActive(false);
        };
    }

    public bool IsPinching()
    {
        return PinchStrength > 0.8F;
    }
    
    public Vector3 GetPinchPosition() {
        return (2 * Thumb.transform.position + IndexFinger.transform.position) * 0.333333F;
    }
}
