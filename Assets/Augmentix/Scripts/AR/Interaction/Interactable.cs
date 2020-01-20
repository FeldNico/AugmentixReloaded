using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private Collider _collider;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
            Debug.LogError("Interactable must have an collider!");
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
