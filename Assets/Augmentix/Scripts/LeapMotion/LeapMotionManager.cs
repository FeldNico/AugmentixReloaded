using System.CodeDom;
using System.Collections;
using System.Linq;
using Augmentix.Scripts;
using ExitGames.Client.Photon;
using Leap;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

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
       
        WaitForPlayer(PlayerType.Primary,CheckUpdateRate, () =>
        {
            Debug.Log("Primary found!");
            base.OnJoinedRoom();
        });
    }

}