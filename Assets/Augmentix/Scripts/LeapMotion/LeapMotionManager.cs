using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Augmentix.Scripts;
using Augmentix.Scripts.LeapMotion;
using ExitGames.Client.Photon;
using Leap;
using Leap.Unity;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class LeapMotionManager : TargetManager
{
    public Player Primary { private get; set; } = null;

    public bool WaitForPrimary = true;
    public float CheckUpdateRate = 0.5f;

    new public void Start()
    {
        base.Start();
        
        PhotonPeer.RegisterType(typeof(Frame), 42, Frame.Serialize, Frame.Deserialize);
        
        OnConnection += () =>
        {
            Camera.main.backgroundColor = Color.green;
        };
    }


    public override void OnJoinedRoom()
    {
        PhotonNetwork.SetInterestGroups((byte)Groups.LEAP_MOTION, true);
        StartCoroutine(CheckForPrimary());

        IEnumerator CheckForPrimary()
        {
            Debug.Log("Waiting for Primary");
            while (true)
            {
                var primary = PhotonNetwork.PlayerListOthers.FirstOrDefault(
                    player => (string) player.CustomProperties["Class"] == PlayerType.Primary.ToString());

                if (primary != null || !WaitForPrimary)
                {
                    Primary = primary;
                    break;
                }

                yield return new WaitForSeconds(CheckUpdateRate);
            }

            Debug.Log("Primary found!");
            base.OnJoinedRoom();
        }
    }

}