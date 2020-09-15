using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
using Augmentix.Scripts.OOI;
using UnityEngine;

public class VirtualCity : MonoBehaviour
{
    public List<(Transform, Renderer, MeshFilter, Material[], float)> RenderList => _renderList;

    private List<(Transform, Renderer, MeshFilter, Material[], float)> _renderList =
        new List<(Transform, Renderer, MeshFilter, Material[], float)>();

    private int childCount = -1;

    private void Start()
    {
#if UNITY_WSA
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            var deskzone = FindObjectOfType<Deskzone>();
            deskzone.Inside += () =>
            {
                foreach (var render in GetComponentsInChildren<Renderer>(true))
                {
                    var ooi = render.GetComponent<OOI>();
                    if (ooi)
                    {
                        var iSphere = ooi.InteractionOrb;
                        if (iSphere)
                        {
                            iSphere.GetComponent<Renderer>().enabled = false;
                            iSphere.GetComponent<Collider>().enabled = false;
                        }
                    }
                    render.enabled = false;
                }
            };
            deskzone.Outside += () =>
            {
                foreach (var render in GetComponentsInChildren<Renderer>(true))
                {
                    if (!render.gameObject.CompareTag("DontRender"))
                    {
                        var ooi = render.GetComponent<OOI>();
                        if (ooi)
                        {
                            var iSphere = ooi.InteractionOrb;
                            if (iSphere)
                            {
                                iSphere.GetComponent<Renderer>().enabled = true;
                                iSphere.GetComponent<Collider>().enabled = true;
                            }
                        }
                    
                        render.enabled = true;
                    }
                }
            };
        }
#endif
    }

    void Update()
    {
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            if (Time.frameCount % 10 == 0)
            {
                var count = CountChildren(transform);
                if (childCount != count)
                {
                    Debug.Log("Child count changed");
                    childCount = count;
                    RefreshRenderList();
                }
            }
        }
    }


    public void RefreshRenderList()
    {
        _renderList.Clear();
        foreach (var child in GetComponentsInChildren<Renderer>(true))
        {
            var mesh = child.GetComponent<MeshFilter>();
            if (mesh == null)
                continue;

            _renderList.Add((child.transform, child, child.GetComponent<MeshFilter>(), child.materials,
                child.transform.parent == null || child.transform.parent.GetComponent<PlayerAvatar>() == null ? 1f : 30f));
        }
    }
    
    private int CountChildren(Transform transform)
    {
        int count = transform.childCount; // direct child count.
        foreach (Transform child in transform)
        {
            count += CountChildren(child); // add child direct children count.
        }

        return count;
    }
}