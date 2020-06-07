﻿using System;
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;


namespace Augmentix.Scripts.VR
{
    public class VRTargetManager: TargetManager
    {

        public float TeleportFadeDuration = 0.1f;
        public float MinMoveDistance = 1f;
        public float PinchStrengh = 0.8f;

        public GameObject AvatarPrefab;

        new void Start()
        {

            base.Start();
            OnConnection += () =>
            {
                PhotonNetwork.SetInterestGroups(new[] {(byte) Groups.LEAP_MOTION}, new[] {(byte) Groups.PLAYERS});
                var avatar =
                    PhotonNetwork.Instantiate(AvatarPrefab != null ? AvatarPrefab.name : "Secondary_Avatar",
                        Camera.main.transform.position, Camera.main.transform.rotation);
                avatar.transform.parent = Camera.main.transform;
                foreach (var child in avatar.GetComponentsInChildren<Renderer>(true))
                {
                    child.enabled = false;
                }
            };
            Connect();


        }
    }
}

