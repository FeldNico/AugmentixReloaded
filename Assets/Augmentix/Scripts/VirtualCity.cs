using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCity : MonoBehaviour
{
    public List<(Transform,Renderer, MeshFilter)> RenderList => _renderList;
    private List<(Transform,Renderer, MeshFilter)> _renderList = new List<(Transform,Renderer, MeshFilter)>();
    private int childCount = -1;
    void Update()
    {
        var count = CountChildren(transform);
        if (childCount != count)
        {
            childCount = count;
            _renderList.Clear();
            foreach (var child in GetComponentsInChildren<Renderer>())
            {
                var mesh = child.GetComponent<MeshFilter>();
                if (mesh == null)
                    continue;

                foreach (var material in child.materials)
                {
                    //material.enableInstancing = true;
                    material.renderQueue = 3002;
                }
                _renderList.Add((child.transform,child,child.GetComponent<MeshFilter>()));
            }
        }
    }
    
    
    private int CountChildren( Transform transform ) {
        int count = transform.childCount;// direct child count.
        foreach(Transform child in transform) {
            count += CountChildren(child);// add child direct children count.
        }
        return count;
    }
}
