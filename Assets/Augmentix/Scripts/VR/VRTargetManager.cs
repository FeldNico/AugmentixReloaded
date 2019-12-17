﻿using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;


namespace Augmentix.Scripts.VR
{
    public class VRTargetManager: TargetManager
    {

        public float TeleportFadeDuration = 0.1f;

        public float MinMoveDistance = 1f;
        
        new void Start()
        {
            base.Start();

            OnConnection += () =>
            {

            };
            
        }

        public override void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                default:
                {
                    Debug.Log("Unkown Event recieved: "+photonEvent.Code+" "+photonEvent.CustomData);
                    break;
                }
            }
        }
    }
}

