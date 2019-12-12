
using ExitGames.Client.Photon;
using Leap;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
        public Transform LeapMotionOffset;
        public Transform PalmOffset;
        
        new void Start()
        {
            base.Start();

            PhotonPeer.RegisterType(typeof(Frame), 42, Frame.Serialize, Frame.Deserialize);
            
            OnConnection += () =>
            {
                PhotonNetwork.SetInterestGroups((byte)Groups.LEAP_MOTION, true);
            };
        }
    }
}

