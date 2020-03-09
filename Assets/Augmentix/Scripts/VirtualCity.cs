using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using UnityEngine;

public class VirtualCity : MonoBehaviour
{
    public List<(Transform, Renderer, MeshFilter, Material[], float)> RenderList => _renderList;

    private List<(Transform, Renderer, MeshFilter, Material[], float)> _renderList =
        new List<(Transform, Renderer, MeshFilter, Material[], float)>();

    private int childCount = -1;

    private void Start()
    {
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            var deskzone = FindObjectOfType<Deskzone>();
            deskzone.Inside += () =>
            {
                foreach (var child in GetComponentsInChildren<Renderer>())
                {
                    child.enabled = false;
                }
            };
            deskzone.Outside += () =>
            {
                foreach (var child in GetComponentsInChildren<Renderer>())
                {
                    child.enabled = true;
                }
            };
        }
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
                    _renderList.Clear();
                    foreach (var child in GetComponentsInChildren<Renderer>())
                    {
                        var mesh = child.GetComponent<MeshFilter>();
                        if (mesh == null)
                            continue;

                        _renderList.Add((child.transform, child, child.GetComponent<MeshFilter>(), child.materials,
                            child.GetComponent<PlayerAvatar>() == null ? 1f : 10f));
                    }
                }
            }
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