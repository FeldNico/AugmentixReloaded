using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Video;

public class InteractionSphere : MonoBehaviour
{
#if UNITY_WSA

    public Dictionary<GameObject, OOI.InteractionFlag> MenuItems { private set; get; } =
        new Dictionary<GameObject, OOI.InteractionFlag>();

    private Transform _ooiTransform;
    private Collider _collider;
    private Vector3 _offset;
    private MeshFilter _mesh;

    private OOI _ooi;
    private WarpzoneManager _warpzoneManager;
    private InteractionManager _interactionManager;
    private ObjectManipulator _manipulator;

    public bool IsInteractedWith { private set; get; } = false;

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

    void Start()
    {
        _warpzoneManager = FindObjectOfType<WarpzoneManager>();
        _interactionManager = FindObjectOfType<InteractionManager>();
        _manipulator = GetComponent<ObjectManipulator>();

        _manipulator.OnManipulationStarted.AddListener(OnManipulationStart);
        _manipulator.OnManipulationEnded.AddListener(OnManipulationEnd);
    }

    private void OnManipulationStart(ManipulationEventData eventData)
    {
        IsInteractedWith = true;
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

        foreach (var flag in interactionList)
        {
            GameObject item = null;
            switch (flag)
            {
                case OOI.InteractionFlag.Text:
                {
                    item = Instantiate(_interactionManager.TextPrefab,
                        transform.position,
                        transform.rotation * Quaternion.AngleAxis(180f, Vector3.up));
                    item.GetComponent<TextMeshPro>().text = _ooi.Text;
                    item.transform.localScale = transform.lossyScale;
                    break;
                }
                case OOI.InteractionFlag.Video:
                {
                    item = Instantiate(_interactionManager.VideoPrefab,
                        transform.position,
                        transform.rotation);
                    var video = _ooi.GetComponent<VideoPlayer>();
                    var scale = transform.lossyScale;
                    scale.y *= (1f * video.height) / video.width;
                    item.transform.localScale = scale;
                    video.targetMaterialRenderer = item.GetComponent<Renderer>();
                    for (ushort i = 0; i < video.audioTrackCount; i++)
                    {
                        video.SetDirectAudioMute(i, true);
                    }

                    video.Stop();
                    video.Play();
                    break;
                }
                case OOI.InteractionFlag.Highlight:
                {
                    item = Instantiate(_interactionManager.HighlightPrefab,
                        transform.position,
                        transform.rotation);
                    item.transform.localScale = transform.lossyScale;
                    break;
                }
            }

            MenuItems.Add(item, flag);
        }

        var camera = Camera.main.transform;

        var count = 0;
        foreach (var menuItem in MenuItems.Keys)
        {
            var x = (float) (_interactionManager.RadianUIRadius * Math.Cos(2 * count * Math.PI / MenuItems.Count));
            var y = (float) (_interactionManager.RadianUIRadius * Math.Sin(2 * count * Math.PI / MenuItems.Count));

            menuItem.transform.position = transform.position + camera.up * x + camera.right * y;
            menuItem.transform.LookAt(camera);
            //menuItem.transform.parent = _warpzoneManager.ActiveWarpzone.transform;
            count++;
        }
    }

    private void OnManipulationEnd(ManipulationEventData eventData)
    {
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

        StartCoroutine(StopMovement());

        IEnumerator StopMovement()
        {
            yield return null;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        IsInteractedWith = false;
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

                    var center = rotation * _ooiTransform.TransformPoint(_halfXRotation * bounds.center) * scale +
                                 position;
                    center.y += (_collider.ClosestPoint(
                        new Vector3(bounds.center.x, float.MaxValue,
                            bounds.center.z)).y + 0.1f) * scale;
                    transform.position = center;
                }

                transform.localScale = Vector3.one * _interactionManager.InteractionSphereScale;
            }
        }
        else
        {
            var center = _collider.bounds.center;
            transform.position = center + (new Vector3(center.x,
                _collider.ClosestPoint(new Vector3(center.x, float.MaxValue,
                        center.z))
                    .y + 0.07f, center.z) - center);
        }
    }
#endif
}