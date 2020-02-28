using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.Network;
using Photon.Pun;
using UnityEngine;

public class SpawnableTangible : MonoBehaviour
{
    public List<GameObject> Spawnables;
    
    public Imposter CurrentSpawnable {private set; get; }

    public void Start()
    {
        foreach (var spawnable in Spawnables)
        {
            var spawn = Instantiate(spawnable,spawnable.transform.localPosition,spawnable.transform.localRotation, transform);
            spawn.transform.localScale = spawnable.transform.localScale * 0.7f;
            Destroy(spawn.GetComponent<AugmentixTransformView>());
            Destroy(spawn.GetComponent<PhotonView>());
            Destroy(spawn.GetComponent<AbstractInteractable>());
            CurrentSpawnable = spawn.AddComponent<Imposter>();
            CurrentSpawnable.Object = spawnable;
        }
    }
}
