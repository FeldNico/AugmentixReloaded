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
    public bool IsDetected {private set; get; } = false;
    public UnityAction OnDetect;
    public UnityAction OnLost;

    private void Awake()
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
}
