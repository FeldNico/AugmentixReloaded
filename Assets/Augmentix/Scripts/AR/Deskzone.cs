using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class Deskzone : MonoBehaviour
{
    private Transform _mainCameraTransform;
    private Renderer _renderer;
    private BoxCollider _collider;
    private Vector3 _colliderHalfSize;

    public UnityAction Inside;
    public UnityAction Outside;

    public Vector3 Position
    {
        get
        {
            var pos = transform.position;
            return new Vector3(pos.x,pos.y - _height,pos.z );
        }
    }

    private float _height;
    public bool IsInside => _inside == 1;
    private short _inside = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<BoxCollider>();
        _colliderHalfSize = _collider.size * 0.5f;
        _mainCameraTransform = Camera.main.transform;
        _height = _renderer.bounds.extents.y;
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && Time.frameCount % 10 == 0)
        {
            if (IsWorldPointInside(_mainCameraTransform.position))
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
    
    public bool IsWorldPointInside (Vector3 point)
    {
        point = _collider.transform.InverseTransformPoint( point ) - _collider.center;
        
        if( point.x < _colliderHalfSize.x && point.x > -_colliderHalfSize.x && 
            point.y < _colliderHalfSize.y && point.y > -_colliderHalfSize.y && 
            point.z < _colliderHalfSize.z && point.z > -_colliderHalfSize.z )
            return true;
        else
            return false;
    }
}
