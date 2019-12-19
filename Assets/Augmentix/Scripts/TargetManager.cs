using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace Augmentix.Scripts
{
    public abstract class TargetManager : MonoBehaviourPunCallbacks, IOnEventCallback
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
            LEAP_MOTION = 0x1
        }

        public enum EventCode : byte
        {
            SEND_IP = 0x42
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
            //PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"Class", Type.ToString()}});
            PhotonNetwork.JoinOrCreateRoom(ROOMNAME, new RoomOptions {MaxPlayers = 0}, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            StartCoroutine(OnConnect());

            IEnumerator OnConnect()
            {
                yield return new WaitForSeconds(0.5f);
                Connected = true;
                OnConnection.Invoke();
            }
        }

        public abstract void OnEvent(EventData photonEvent);
    }
}