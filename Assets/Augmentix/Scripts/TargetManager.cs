using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace Augmentix.Scripts
{
    public abstract class TargetManager : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
    {
        public enum PlayerType
        {
            Unkown,
            Primary,
            Secondary,
            LeapMotion
        }

        public enum Groups : byte
        {
            LEAP_MOTION = 0x1,
            PLAYERS = 0x2
        }

        public enum EventCode : byte
        {
            SEND_IP = 0x42,
            HAND_LOST = 0x43,
            EXTENDED = 0x44,
            HIGHLIGHT = 0x45,
            NAVIGATION = 0x46,
            POINTING = 0x47
        }
    
        public static TargetManager Instance { protected set; get; }

        public const string ROOMNAME = "AUGMENTIX";

        public PlayerType Type = PlayerType.Unkown;

        public bool Connected { private set; get; } = false;
        public UnityAction OnConnection;

        protected const string gameVersion = "1";

        public void Awake()
        {
            if (Instance == null)
                Instance = this;

            Debug.unityLogger.logEnabled = false;

            PhotonNetwork.AutomaticallySyncScene = false;
        }

        public void Start()
        {
            if (Type == PlayerType.Unkown)
            {
                Debug.LogError("No Classname set!");
                return;
            }
            
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            PhotonNetwork.GameVersion = gameVersion;

        }

        public void Connect()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"Class", Type.ToString()}});
            PhotonNetwork.JoinOrCreateRoom(ROOMNAME, new RoomOptions {MaxPlayers = 0}, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            switch (Type)
            {
                case PlayerType.Primary:
                {
                    PhotonNetwork.SetInterestGroups(null, new []{(byte)Groups.LEAP_MOTION,(byte)Groups.PLAYERS});
                    break;
                }
                case PlayerType.Secondary:
                {
                    PhotonNetwork.SetInterestGroups(new [] {(byte)Groups.LEAP_MOTION}, new []{(byte)Groups.PLAYERS});
                    break;
                }
                case PlayerType.LeapMotion:
                {
                    PhotonNetwork.SetInterestGroups(new [] {(byte)Groups.PLAYERS}, new []{(byte)Groups.LEAP_MOTION});
                    break;
                }
            }
            
            StartCoroutine(OnConnect());

            IEnumerator OnConnect()
            {
                yield return new WaitForSeconds(0.5f);
                Connected = true;
                OnConnection.Invoke();
            }
        }

        public void WaitForPlayer(PlayerType playerType,float CheckUpdateRate, Action callback)
        {
            StartCoroutine(CheckForPrimary());

            IEnumerator CheckForPrimary()
            {
                Player player = null;
                while (true)
                {
                    player = PhotonNetwork.PlayerListOthers.FirstOrDefault(
                        p => (string) p.CustomProperties["Class"] == playerType.ToString());
                    
                    if (player != null)
                        break;
                    
                    yield return new WaitForSeconds(CheckUpdateRate);
                }

                callback.Invoke();
            }
        }


        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            var ooi = targetView.GetComponent<OOI.OOI>();
            if (requestingPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && ooi)
            {
                if (Type == PlayerType.Primary)
                {
                    if (!ooi.IsBeingManipulated)
                    {
                        targetView.TransferOwnership(requestingPlayer);
                    }
                    else
                    {
                        targetView.RPC("OnTransferDenied",requestingPlayer);
                    }
                }
                else
                {
                    if ((string) requestingPlayer.CustomProperties["Class"] == PlayerType.Primary.ToString())
                    {
                        #if UNITY_ANDROID
                        if (ooi.IsBeingManipulated)
                        {
                            ooi.OnTransferDenied();
                        }
                        #endif
                        targetView.TransferOwnership(requestingPlayer);
                    }
                    else
                    {
                        if (!ooi.IsBeingManipulated)
                        {
                            targetView.TransferOwnership(requestingPlayer);
                        }
                        else
                        {
                            targetView.RPC("OnTransferDenied",requestingPlayer);
                        }
                    }
                }
            }
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            
        }
    }
}