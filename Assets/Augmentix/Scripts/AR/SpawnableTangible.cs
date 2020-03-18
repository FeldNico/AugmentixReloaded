using System;
using System.Collections;
using System.Collections.Generic;
using Augmentix.Scripts.Network;
using Augmentix.Scripts.OOI;
using Photon.Pun;
using UnityEngine;

public class SpawnableTangible : MonoBehaviour
{
    public float Scale = 0.06f * 0.7f;
    
    public List<GameObject> Spawnables;
    
    public Imposter CurrentSpawnable {private set; get; }

    public void Start()
    {
        foreach (var spawnable in Spawnables)
        {
            var spawn = Instantiate(spawnable,spawnable.transform.localPosition,spawnable.transform.localRotation, transform);
            spawn.transform.localScale = spawnable.transform.localScale * Scale;
            CurrentSpawnable = spawn.AddComponent<Imposter>();
            CurrentSpawnable.Object = spawnable;
        }
    }
}
