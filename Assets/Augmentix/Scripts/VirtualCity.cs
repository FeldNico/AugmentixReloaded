using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts;
using UnityEngine;

public class VirtualCity : MonoBehaviour
{
    public List<(Transform,Renderer, MeshFilter,Material[])> RenderList => _renderList;
    private List<(Transform,Renderer, MeshFilter,Material[])> _renderList = new List<(Transform,Renderer, MeshFilter,Material[])>();
    private int childCount = -1;
    void Update()
    {
        if (TargetManager.Instance.Type == TargetManager.PlayerType.Primary)
        {
            if (Time.frameCount % 10 == 0)
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
                
                        _renderList.Add((child.transform,child,child.GetComponent<MeshFilter>(),child.materials));
                    }
                }
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
