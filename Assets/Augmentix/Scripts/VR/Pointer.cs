using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour
{
    
    public GameObject Dot;
    public PointerTarget CurrentPointerTarget { private set; get; }

    private LineRenderer lineRenderer;
    private GameObject _vrmap;
    private float MaxLength = 5f;
    // Start is called before the first frame update
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        /*
        OnClick.AddOnStateDownListener((action, source) =>
        {
            if (CurrentPointerTarget == null)
                return;

            var pos = CurrentPointerTarget.transform.InverseTransformPoint(Dot.transform.position);
            
            CurrentPointerTarget.OnPress?.Invoke(new Vector2(pos.x,pos.z));
        },SteamVR_Input_Sources.RightHand);
        
        OnClick.AddOnStateUpListener((action, source) =>
        {
            if (CurrentPointerTarget == null)
                return;

            var pos = CurrentPointerTarget.transform.InverseTransformPoint(Dot.transform.position);
            
            CurrentPointerTarget.OnRelease?.Invoke(new Vector2(pos.x,pos.z));
        },SteamVR_Input_Sources.RightHand);
        */
    }

    // Update is called once per frame
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            _vrmap = PhotonNetwork.Instantiate("MapCanvas", Vector3.zero, Quaternion.identity);
            var rectTransform = _vrmap.GetComponent<RectTransform>();
            rectTransform.parent = GameObject.Find("LeftHandAnchor").transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f,0.5f);
            rectTransform.localPosition = new Vector3(0,0.051f,0.409f);
            rectTransform.localRotation = Quaternion.Euler(60.508f,-3.861f,356.04f);
            rectTransform.localScale = new Vector3(0.05f,0.05f,0.05f);
        }

        if (OVRInput.GetUp(OVRInput.RawButton.X))
        {
            PhotonNetwork.Destroy(_vrmap);
        }
        
        if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (CurrentPointerTarget == null)
                return;

            var pos = CurrentPointerTarget.transform.InverseTransformPoint(Dot.transform.position);
            
            CurrentPointerTarget.OnRelease?.Invoke(new Vector2(pos.x,pos.z));
        }
        
        UpdateLine();
    }

    private void UpdateLine()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position,transform.forward, out hit, MaxLength,LayerMask.GetMask("UI"));

        if (hit.collider != null)
        {
            var pointerTarget = hit.transform.GetComponent<PointerTarget>();

            if (pointerTarget == null)
            {
                Dot.SetActive(false);
                lineRenderer.enabled = false;
                return;
            }
            Dot.SetActive(true);
            lineRenderer.enabled = true;
            Dot.transform.position = hit.point;
            lineRenderer.SetPosition(0,transform.position);
            lineRenderer.SetPosition(1,Dot.transform.position);

            if (pointerTarget != CurrentPointerTarget)
            {
                if (CurrentPointerTarget != null)
                {
                    CurrentPointerTarget.OnHoverEnd?.Invoke();
                }
                pointerTarget.OnHoverStart?.Invoke();
                CurrentPointerTarget = pointerTarget;
            }
        }
        else
        {
            Dot.SetActive(false);
            lineRenderer.enabled = false;

            if (CurrentPointerTarget != null)
            {
                CurrentPointerTarget.OnHoverEnd?.Invoke();
                //Dot.gameObject.SetActive(false);
            }
            CurrentPointerTarget = null;
        }
    }
}
