using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof(SphereCollider), typeof(SphereCollider))]
public class PinchingSphere : MonoBehaviour
{
    private List<Collider> _collider = new List<Collider>();

    public AbstractInteractable CurrentInteractable
    {
        get
        {
            if (IsColliding())
                return _collider[0].GetComponent<AbstractInteractable>();
            
            return null;
        }
    }

    public bool IsColliding()
    {
        return _collider.Count != 0;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<AbstractInteractable>())
            return;
        
        if (!_collider.Contains(other))
        {
            _collider.Add(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<AbstractInteractable>())
            return;
        
        if (_collider.Contains(other))
        {
            _collider.Remove(other);
        }
    }
    
}
