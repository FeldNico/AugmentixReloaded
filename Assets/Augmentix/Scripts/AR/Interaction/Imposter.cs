﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Augmentix.Scripts;
using Augmentix.Scripts.Network;
using Augmentix.Scripts.OOI;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Imposter : MonoBehaviour
{
    public GameObject Object;

    private ObjectManipulator _manipulator = null;
    private MixedRealityInputSystem _inputSystem = null;
    private VirtualCity _virtualCity;
    private void Awake()
    {
        _inputSystem = FindObjectOfType<MixedRealityToolkit>().GetService<MixedRealityInputSystem>();
        _virtualCity = FindObjectOfType<VirtualCity>();
        
        var ooi = GetComponent<OOI>();
        if (ooi)
            Destroy(ooi);
        Destroy(GetComponent<AugmentixTransformView>());
        Destroy(GetComponent<PhotonView>());
        
        _manipulator = gameObject.AddComponent<ObjectManipulator>();
        _manipulator.ReleaseBehavior = 0;
        gameObject.AddComponent<NearInteractionGrabbable>();
        gameObject.AddComponent<MinMaxScaleConstraint>();
        _manipulator.OnManipulationStarted.AddListener(OnManipulation);
        _manipulator.OnManipulationEnded.AddListener(data =>
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        });
    }

    void OnManipulation(ManipulationEventData data)
    {
        if (FindObjectOfType<WarpzoneManager>().ActiveWarpzone != null)
        {
            var obj = PhotonNetwork.Instantiate(Path.Combine("OOI","Spawnable",Object.name), transform.position, transform.rotation,
                (byte) TargetManager.Groups.PLAYERS);
            obj.transform.parent = _virtualCity.transform;
            obj.transform.localScale = transform.lossyScale;

            var pointer = data.Pointer as SpherePointer;
            var inputAction = pointer.PoseAction;
            var handedness = pointer.Handedness;
            StartCoroutine(EndManipulation());
            IEnumerator EndManipulation()
            {
                yield return null;
                _manipulator.ForceEndManipulation();
                _manipulator.enabled = false;
                _manipulator.GetComponent<Collider>().enabled = false;
                pointer.IsFocusLocked = false;
                yield return null;
                _inputSystem.RaisePointerDown(pointer,inputAction,handedness);
                _manipulator.enabled = true;
                _manipulator.GetComponent<Collider>().enabled = true;
            }
        }
    }
}
