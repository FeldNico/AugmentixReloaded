using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Video;

public class InteractionSphere : AbstractInteractable
{
    #if UNITY_WSA
    
    public Dictionary<GameObject,OOI.InteractionFlag> MenuItems { private set; get; } = new Dictionary<GameObject, OOI.InteractionFlag>();
    
    private Transform _ooiTransform;
    private Collider _collider;
    private Vector3 _offset;
    private MeshFilter _mesh;

    private OOI _ooi;
    private WarpzoneManager _warpzoneManager;
    private InteractionManager _interactionManager;

    public OOI OOI
    {
        set
        {
            _ooi = value;
            _ooiTransform = _ooi.transform;
            _collider = _ooi.GetComponent<Collider>();
            _mesh = _ooi.GetComponent<MeshFilter>();
        }
        get => _ooi;
    }

    new void Start()
    {
        base.Start();
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _interactionManager = FindObjectOfType<InteractionManager>();
        
        OnInteractionStart += (hand) =>
        {
            var interactionList = new List<OOI.InteractionFlag>();
            if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Text))
            {
                interactionList.Add(OOI.InteractionFlag.Text);
            }
            if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Video))
            {
                interactionList.Add(OOI.InteractionFlag.Video);
            }
            if (_ooi.Flags.HasFlag(OOI.InteractionFlag.Highlight))
            {
                interactionList.Add(OOI.InteractionFlag.Highlight);
            }
            
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint == null)
                joint = gameObject.AddComponent<FixedJoint>();

            joint.connectedBody = hand.PinchingSphere.GetComponent<Rigidbody>();
            
            foreach (var flag in interactionList)
            {
                GameObject item = null;
                switch (flag)
                {
                    case OOI.InteractionFlag.Text:
                    {
                        item = Instantiate(_interactionManager.TextPrefab,_warpzoneManager.ActiveWarpzone.transform.position,_warpzoneManager.ActiveWarpzone.transform.rotation * Quaternion.AngleAxis(180f, Vector3.up), transform);
                        item.GetComponent<TextMeshPro>().text = _ooi.Text;
                        break;
                    }
                    case OOI.InteractionFlag.Video:
                    {
                        item = Instantiate(_interactionManager.VideoPrefab,_warpzoneManager.ActiveWarpzone.transform.position,_warpzoneManager.ActiveWarpzone.transform.rotation, transform);
                        var video = _ooi.GetComponent<VideoPlayer>();
                        var scale = item.transform.localScale;
                        scale.y *= (1f * video.height) / video.width;
                        item.transform.localScale = scale;
                        video.targetMaterialRenderer = item.GetComponent<Renderer>();
                        for (ushort i = 0; i < video.audioTrackCount; i++)
                        {
                            video.SetDirectAudioMute( i,true);
                        }
                        video.Stop();
                        video.Play();
                        break;
                    }
                    case OOI.InteractionFlag.Highlight:
                    {
                        item = Instantiate(_interactionManager.HighlightPrefab,_warpzoneManager.ActiveWarpzone.transform.position,_warpzoneManager.ActiveWarpzone.transform.rotation, transform);
                        break;
                    }
                }
                
                MenuItems.Add(item,flag);
            }

            var camera = Camera.main.transform;
            
            var count = 0;
            foreach (var menuItem in MenuItems.Keys)
            {
                var x = (float) (_interactionManager.RadianUIRadius  * Math.Cos(2 * count * Math.PI / MenuItems.Count));
                var y = (float) (_interactionManager.RadianUIRadius  * Math.Sin(2 * count * Math.PI / MenuItems.Count));

                menuItem.transform.position = transform.position + camera.up * x + camera.right * y;
                menuItem.transform.LookAt(camera);
                menuItem.transform.parent = _warpzoneManager.ActiveWarpzone.transform;
                count++;
            }
        };

        OnInteractionEnd += (hand) =>
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            if (joint != null && joint.connectedBody == hand.PinchingSphere.GetComponent<Rigidbody>())
                Destroy(joint);
            
            if (_ooi.GetComponent<VideoPlayer>())
                _ooi.GetComponent<VideoPlayer>().Stop();
            
            foreach (var key in MenuItems.Keys)
            {
                if (key.GetComponent<SphereCollider>().bounds.Contains(transform.position))
                {
                    _ooi.Interact(MenuItems[key]);
                }
                Destroy(key);
            }
            MenuItems.Clear();
        };
        
    }

    private Quaternion _halfXRotation = Quaternion.AngleAxis(0, Vector3.up);
    void Update()
    {
        if (OOI == null || !_collider.enabled || IsInteractedWith)
            return;

        if (_ooi.StaticOOI)
        {
            if (_warpzoneManager.ActiveWarpzone != null)
            {
                var warpzone = _warpzoneManager.ActiveWarpzone;
                if (warpzone.Matrices.ContainsKey(_ooiTransform))
                {
                    var m = warpzone.Matrices[_ooiTransform];
                    var bounds = _mesh.mesh.bounds;
                    var position = new Vector3(m[0, 3], m[1, 3], m[2, 3]);
                    var scale = m.GetColumn(0).magnitude;
                    var rotation = Quaternion.LookRotation(
                        m.GetColumn(2),
                        m.GetColumn(1)
                    );
                    var center = rotation * _ooiTransform.TransformVector( _halfXRotation * bounds.center) * scale + position;
                    center.y += (_collider.ClosestPoint(Quaternion.Inverse(rotation) * new Vector3(bounds.center.x, float.MaxValue, bounds.center.z)).y + 0.1f) * scale;
                    transform.position = center;
                }

                transform.localScale = Vector3.one * _interactionManager.InteractionSphereScale;
            }
        }
        else
        {
            transform.localScale = new Vector3(_interactionManager.InteractionSphereScale / transform.lossyScale.x, _interactionManager.InteractionSphereScale / transform.lossyScale.y,
                _interactionManager.InteractionSphereScale/ transform.lossyScale.z);
            var center = _collider.bounds.center;
            transform.position = center + (new Vector3(center.x,
                                               _collider.ClosestPoint(new Vector3(center.x, float.MaxValue, center.z))
                                                   .y + 0.07f, center.z) - center);
        }
    }
#endif
}