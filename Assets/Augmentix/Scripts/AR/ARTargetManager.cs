using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using UnityEngine.XR.WSA;
using Vuforia;
#if UNITY_WSA
using System.Collections;
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
        public int Port = 1337;
        //public ARHands Hands;
        public GameObject AvatarPrefab;
        public GameObject WarpzoneDummyPrefab;
        [HideInInspector]
        public bool DoCalibrate = false;
        [HideInInspector]
        public Vector3 FirstCalibrationVector, SecondCalibrationVector;
        public TMP_Text DebugText;

        private Deskzone _deskzone;
        private WarpzoneManager _warpzoneManager;
        private VirtualCity _virtualCity;

        new public void Awake()
        {
            if (AvatarPrefab == null)
                AvatarPrefab = Resources.Load<GameObject>("Primary_Avatar");
            /*
            if (Hands == null)
                Hands = FindObjectOfType<ARHands>();
                */
            if (DebugText == null)
                DebugText = FindObjectOfType<TMP_Text>();

            _warpzoneManager = FindObjectOfType<WarpzoneManager>();
            _virtualCity = FindObjectOfType<VirtualCity>();
            _deskzone = FindObjectOfType<Deskzone>();
            
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
            OnConnection += () =>
            {
                if (AvatarPrefab != null)
                    Debug.Log(AvatarPrefab.name);
                
                var avatar = PhotonNetwork.Instantiate(AvatarPrefab.name, Camera.main.transform.position,
                    Camera.main.transform.rotation);
                
                avatar.transform.parent = Camera.main.transform;
                avatar.GetComponent<Renderer>().enabled = false;
            };
            
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(() =>
                {
                    var fps = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.FAST |
                                                               VuforiaRenderer.FpsHint.NO_VIDEOBACKGROUND);
                    Debug.Log("Recommended Fps: "+fps);
                    Application.targetFrameRate = fps;
                });
        }

        public void SetupDeskzoneAndConnect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                var target = _deskzone.transform.parent;
                _deskzone.transform.parent = null;
                target.gameObject.SetActive(false);
                var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
                objectTracker.Stop();
                objectTracker.DeactivateDataSet(objectTracker.GetDataSets().First(set => set.Path == "Vuforia/Augmentix_Floor.xml"));
                objectTracker.Start();
                _deskzone.gameObject.AddComponent<WorldAnchor>();
                Connect();
            }
        }
    }
}