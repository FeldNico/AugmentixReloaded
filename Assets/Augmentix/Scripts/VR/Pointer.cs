using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour
{
    
    public GameObject Dot;
    public PointerTarget CurrentPointerTarget { private set; get; }

    private LineRenderer lineRenderer;
    private VRMap _vrmap;
    private float MaxLength = 5f;
    // Start is called before the first frame update
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        _vrmap = FindObjectOfType<VRMap>();
        _vrmap.gameObject.SetActive(false);

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
            _vrmap.gameObject.SetActive(true);
        }

        if (OVRInput.GetUp(OVRInput.RawButton.X))
        {
            _vrmap.gameObject.SetActive(false);
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
        var ray = new Ray(transform.position,transform.forward);
        Physics.Raycast(ray, out hit, MaxLength);

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
