using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof(SphereCollider), typeof(SphereCollider))]
public class PinchingSphere : MonoBehaviour
{
    private List<Collider> _collider = new List<Collider>();

    public Interactable CurrentInteractable
    {
        get
        {
            if (IsColliding())
                return _collider[0].GetComponent<Interactable>();
            
            return null;
        }
    }

    public bool IsColliding()
    {
        return _collider.Count != 0;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Interactable>())
            return;
        
        if (!_collider.Contains(other))
        {
            _collider.Add(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<Interactable>())
            return;
        
        if (_collider.Contains(other))
        {
            _collider.Remove(other);
        }
    }
    
}
