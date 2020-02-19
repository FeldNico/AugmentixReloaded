using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Deskzone : MonoBehaviour
{
    private Transform _mainCameraTransform;
    private Renderer _renderer;

    public UnityAction Inside;
    public UnityAction Outside;
    
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _mainCameraTransform = Camera.main.transform;
    }

    private short _inside = -1;
    void Update()
    {
        if (_renderer.bounds.Contains(_mainCameraTransform.position))
        {
            if (_inside != 1)
            {
                Inside?.Invoke();
                _inside = 1;
            }
        }
        else
        {
            if (_inside != 0)
            {
                Outside?.Invoke();
                _inside = 0;
            }
        }
    }
}
