using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;


namespace Augmentix.Scripts.VR
{
    public class VRTargetManager: TargetManager
    {

        public float TeleportFadeDuration = 0.1f;
        public float MinMoveDistance = 1f;

        public GameObject AvatarPrefab;

        new void Start()
        {
            base.Start();

            OnConnection += () =>
            {
                var avatar =
                    PhotonNetwork.Instantiate(AvatarPrefab.name, transform.position, transform.rotation);
                avatar.GetComponent<Renderer>().enabled = false;
                avatar.transform.parent = GameObject.Find("CenterEyeAnchor").transform;
            };
        }
    }
}

