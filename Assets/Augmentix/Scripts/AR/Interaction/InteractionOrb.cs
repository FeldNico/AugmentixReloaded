using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.Network;
using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Video;

public class InteractionOrb : MonoBehaviour
{
#if UNITY_WSA

    public Dictionary<GameObject, object> MenuItems { private set; get; } =
        new Dictionary<GameObject, object>();

    private Transform _targetTransform;
    private Collider _collider;
    private Vector3 _offset;
    private MeshFilter _mesh;

    private MonoBehaviour _target;
    private WarpzoneManager _warpzoneManager;
    private InteractionManager _interactionManager;
    private ObjectManipulator _manipulator;

    public bool IsInteractedWith { private set; get; } = false;

    public MonoBehaviour Target
    {
        set
        {
            _target = value;
            _targetTransform = _target.transform;
            _collider = _target.GetComponent<Collider>();
            _mesh = _target.GetComponent<MeshFilter>();
        }
        get => _target;
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
        if (_target is OOI)
        {
            var ooi = (OOI) _target;

            var interactionList = new List<OOI.InteractionFlag>();
            if (ooi.Flags.HasFlag(OOI.InteractionFlag.Text))
            {
                interactionList.Add(OOI.InteractionFlag.Text);
            }

            if (ooi.Flags.HasFlag(OOI.InteractionFlag.Video))
            {
                interactionList.Add(OOI.InteractionFlag.Video);
            }

            if (ooi.Flags.HasFlag(OOI.InteractionFlag.Highlight))
            {
                interactionList.Add(OOI.InteractionFlag.Highlight);
            }

            if (ooi.Flags.HasFlag(OOI.InteractionFlag.Delete))
            {
                interactionList.Add(OOI.InteractionFlag.Delete);
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
                        item.GetComponent<TextMeshPro>().text = ooi.Text;
                        item.transform.localScale = transform.lossyScale;
                        break;
                    }
                    case OOI.InteractionFlag.Video:
                    {
                        item = Instantiate(_interactionManager.VideoPrefab,
                            transform.position,
                            transform.rotation);
                        var video = ooi.GetComponent<VideoPlayer>();
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
                    case OOI.InteractionFlag.Delete:
                    {
                        item = Instantiate(_interactionManager.DeletePrefab,
                            transform.position,
                            transform.rotation);
                        item.transform.localScale = transform.lossyScale;
                        break;
                    }
                }

                MenuItems.Add(item, flag);
            }
        }
        else if (_target is SpawnableTangible)
        {
            var tangible = (SpawnableTangible) _target;
            foreach (var spawnable in tangible.Spawnables)
            {
                var go = Instantiate(spawnable, transform.position, transform.rotation,transform);
                go.transform.localPosition = spawnable.transform.localPosition;
                go.transform.localRotation = spawnable.transform.localRotation;
                var ooi = go.GetComponent<OOI>();
                if (ooi)
                    Destroy(ooi);
                Destroy(go.GetComponent<AugmentixTransformView>());
                Destroy(go.GetComponent<PhotonView>());
                
                var bounds = go.GetComponent<Renderer>().bounds;
                var max = Math.Max(bounds.extents.x, Math.Max(bounds.extents.y, bounds.extents.z));
                ooi.transform.localScale = go.transform.localScale * (0.018f / max);
                
                MenuItems.Add(go, spawnable);
            }
        }


        var camera = Camera.main.transform;

        var count = 0;
        foreach (var menuItem in MenuItems.Keys)
        {
            var x = (float) (_interactionManager.RadianUIRadius * Math.Cos(2 * count * Math.PI / MenuItems.Count));
            var y = (float) (_interactionManager.RadianUIRadius * Math.Sin(2 * count * Math.PI / MenuItems.Count));

            menuItem.transform.position = transform.position + camera.up * x + camera.right * y;
            menuItem.transform.LookAt(camera);
            count++;
        }
    }

    private void OnManipulationEnd(ManipulationEventData eventData)
    {
        if (_target is OOI)
        {
            if (_target.GetComponent<VideoPlayer>())
                _target.GetComponent<VideoPlayer>().Stop();
        }


        foreach (var key in MenuItems.Keys)
        {
            if (key.GetComponent<Collider>().bounds.Contains(transform.position))
            {
                if (_target is OOI)
                {
                    ((OOI) _target).Interact((OOI.InteractionFlag) MenuItems[key]);
                }
                else if (_target is SpawnableTangible)
                {
                    var tangible = (SpawnableTangible) _target;
                    if (tangible.Imposter)
                        Destroy(tangible.Imposter.gameObject);
                    
                    var spawnable = (GameObject) MenuItems[key];
                    tangible.Imposter = Instantiate(spawnable, Vector3.zero, Quaternion.identity, _target.transform)
                        .AddComponent<Imposter>();
                    tangible.Imposter.transform.localPosition = spawnable.transform.localPosition;
                    tangible.Imposter.transform.localRotation = spawnable.transform.localRotation;
                    tangible.Imposter.transform.localScale = spawnable.transform.localScale * tangible.Scale;
                    tangible.Imposter.Object = spawnable;
                }
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
        if (Target == null || !_collider.enabled || IsInteractedWith)
            return;

        if (_target is OOI && ((OOI) _target).StaticOOI)
        {
            if (_warpzoneManager.ActiveWarpzone != null)
            {
                var warpzone = _warpzoneManager.ActiveWarpzone;
                if (warpzone.Matrices.ContainsKey(_targetTransform))
                {
                    var m = warpzone.Matrices[_targetTransform];
                    var bounds = _mesh.sharedMesh.bounds;
                    var position = new Vector3(m[0, 3], m[1, 3], m[2, 3]);
                    var scale = m.GetColumn(0).magnitude;
                    var rotation = Quaternion.LookRotation(
                        m.GetColumn(2),
                        m.GetColumn(1)
                    );

                    var center = rotation * bounds.center * scale +
                                 position;
                    center.y += (_collider.ClosestPoint(
                        new Vector3(_collider.bounds.center.x, float.MaxValue,
                            _collider.bounds.center.z)).y * scale) + 0.07f;
                    transform.position = center;
                }

                transform.localScale = Vector3.one * _interactionManager.InteractionSphereScale;
            }
        }
        else
        {
            var center = _collider.bounds.center;
            transform.position = new Vector3(center.x,
                _collider.ClosestPoint(new Vector3(center.x, float.MaxValue,
                        center.z))
                    .y + 0.1f, center.z);
        }
    }

    private void OnDestroy()
    {
        foreach (var key in MenuItems.Keys)
        {
            Destroy(key);
        }
    }
#endif
}