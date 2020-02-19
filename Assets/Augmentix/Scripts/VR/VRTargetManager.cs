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
                    PhotonNetwork.Instantiate(AvatarPrefab != null ? AvatarPrefab.name : "Secondary_Avatar", Camera.main.transform.position, Camera.main.transform.rotation);
                avatar.transform.parent = Camera.main.transform;
                avatar.GetComponent<Renderer>().enabled = false;
            };
            Connect();
        }
    }
}

