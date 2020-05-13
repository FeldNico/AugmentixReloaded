using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts.Network;
using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;

public class SpawnableTangible : MonoBehaviour
{
    public float Scale = 0.06f * 0.7f;

    public List<GameObject> Spawnables;
    [HideInInspector]
    public Imposter Imposter;

    private InteractionManager _interactionManager;
    private GridObjectCollection _collection;
    private Interactable _interactable;
    private InteractionOrb _interactionOrb;

    public void Start()
    {
        if (Spawnables.Count == 0)
            return;
        
        _interactionManager = FindObjectOfType<InteractionManager>();

        if (Spawnables.Count == 1)
        {
            var spawnable = Spawnables[0];
            Imposter = Instantiate(spawnable, Vector3.zero, Quaternion.identity, transform)
                .AddComponent<Imposter>();
            Imposter.transform.localPosition = spawnable.transform.localPosition;
            Imposter.transform.localRotation = spawnable.transform.localRotation;
            Imposter.transform.localScale = spawnable.transform.localScale * Scale;
            Imposter.Object = spawnable;
        }
        else
        {
            var prefab = FindObjectOfType<InteractionManager>().InteractionOrbPrefab;
                
            _interactionOrb =
                Instantiate(prefab, transform.position + new Vector3(0,GetComponent<Collider>().bounds.size.y,0),
                    transform.rotation).GetComponent<InteractionOrb>();
            _interactionOrb.Target = this;
            
            GetComponent<DefaultTrackableEventHandler>().OnTargetFound.AddListener(() =>
            {
                _interactionOrb.GetComponent<Renderer>().enabled = true;
                _interactionOrb.GetComponent<Collider>().enabled = true;
            });
            GetComponent<DefaultTrackableEventHandler>().OnTargetLost.AddListener(() =>
            {
                _interactionOrb.GetComponent<Renderer>().enabled = false;
                _interactionOrb.GetComponent<Collider>().enabled = false;
            });
            
            /*
            _collection = new GameObject("Collection").AddComponent<GridObjectCollection>();
            _collection.transform.parent = transform;
            _collection.transform.localPosition = Vector3.zero;
            _collection.transform.localRotation = Quaternion.identity;
            _collection.transform.localScale = Vector3.one;

            _collection.CellWidth = 0.05f;
            _collection.CellHeight = 0.05f;
            _collection.Distance = 0.08f;
            _collection.Anchor = LayoutAnchor.BottomLeft;
            _collection.Layout = LayoutOrder.ColumnThenRow;
            _collection.Columns = 3;

            _interactable = Instantiate(_interactionManager.SpawnableButtonPrefab, Vector3.zero, Quaternion.identity
                , transform).GetComponent<Interactable>();
            _interactable.transform.localPosition = new Vector3(0, 0, -0.06f);
            _interactable.transform.localRotation = Quaternion.Euler(75, 0, 0);
            _interactable.transform.localScale = _interactionManager.SpawnableButtonPrefab.transform.localScale;

            _interactable.OnClick.AddListener(() =>
            {
                _collection.gameObject.SetActive(!_collection.gameObject.activeSelf);
            });

            foreach (var spawnable in Spawnables)
            {
                var spawn = Instantiate(spawnable, Vector3.zero, Quaternion.identity, _collection.transform);
                spawn.transform.localPosition = spawnable.transform.localPosition;
                spawn.transform.localRotation =
                    spawnable.transform.localRotation * Quaternion.AngleAxis(90, Vector3.right);

                var bounds = spawn.GetComponent<Renderer>().bounds;
                var max = Math.Max(bounds.extents.x, Math.Max(bounds.extents.y, bounds.extents.z));

                spawn.transform.localScale = spawn.transform.localScale * (0.018f / max);

                Destroy(spawn.GetComponent<OOI>());
                Destroy(spawn.GetComponent<AugmentixTransformView>());
                Destroy(spawn.GetComponent<PhotonView>());
                if (spawn.GetComponent<VideoPlayer>())
                    Destroy(spawn.GetComponent<VideoPlayer>());
                if (spawn.GetComponent<Rigidbody>())
                    Destroy(spawn.GetComponent<Rigidbody>());
                if (!spawn.GetComponent<Collider>())
                    spawn.gameObject.AddComponent<BoxCollider>();

                spawn.gameObject.AddComponent<NearInteractionTouchable>();
                var button = spawn.gameObject.AddComponent<PressableButton>();
                button.ButtonPressed.AddListener(() =>
                {
                    if (Imposter != null)
                    {
                        Destroy(Imposter.gameObject);
                    }

                    Imposter = Instantiate(spawnable, Vector3.zero, Quaternion.identity, transform)
                        .AddComponent<Imposter>();
                    Imposter.transform.localPosition = spawnable.transform.localPosition;
                    Imposter.transform.localRotation = spawnable.transform.localRotation;
                    Imposter.transform.localScale = spawnable.transform.localScale * Scale;
                    Imposter.Object = spawnable;

                    _collection.gameObject.SetActive(false);
                });
            }

            _collection.UpdateCollection();
            _collection.gameObject.SetActive(false);
            */
        }
    }
}