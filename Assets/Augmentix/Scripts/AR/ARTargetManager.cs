using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Augmentix.Scripts.LeapMotion;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_WSA
using System.Collections;
using Leap;
using Photon.Pun;
using TMPro;
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
#if UNITY_WSA
        public Transform LeapMotionOffset;
        public int Port = 1337;
        public ARHands Hands;
        public bool DoCalibrate = false;
        public Vector3 FirstCalibrationVector, SecondCalibrationVector;
        public TMP_Text DebugText;

        new public void Awake()
        {

            Application.logMessageReceived += (message, trace, type) =>
            {
                if (!DebugText.text.EndsWith(message + "\n"))
                    DebugText.text += message + "\n";

                int count = 0;
                foreach (char c in DebugText.text)
                    if (c == '\n')
                        count++;

                count = count - 35;
                if (count > 0)
                {
                    var index = 0;
                    for (int i = 0; i < count; i++)
                    {
                        index = DebugText.text.IndexOf('\n', index + 1);
                    }

                    DebugText.text = DebugText.text.Substring(index);
                }
            };
            base.Awake();
        }

        new void Start()
        {
            base.Start();

            OnConnection += () => { PhotonNetwork.SetInterestGroups((byte) Groups.LEAP_MOTION, true); };
        }
#endif
        public override void OnEvent(EventData photonEvent)
        {
        }

    }
}