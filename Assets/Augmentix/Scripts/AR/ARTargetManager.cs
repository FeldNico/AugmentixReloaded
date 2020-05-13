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
#if UNITY_WSA
using Vuforia;
using System.Collections;
using UnityEngine.XR.WSA.Input;

#endif

namespace Augmentix.Scripts.AR
{
    public class ARTargetManager : TargetManager
    {
        public GameObject AvatarPrefab;
        public GameObject WarpzoneDummyPrefab;
        public TMP_Text DebugText;

        private Deskzone _deskzone;

        new public void Awake()
        {
            if (AvatarPrefab == null)
                AvatarPrefab = Resources.Load<GameObject>("Primary_Avatar");

            if (DebugText == null)
                DebugText = FindObjectOfType<TMP_Text>();

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

                var avatar = PhotonNetwork.Instantiate(AvatarPrefab.name, Camera.main.transform.position,
                    Camera.main.transform.rotation);

                avatar.transform.parent = Camera.main.transform;
                avatar.GetComponent<Renderer>().enabled = false;
            };
#if UNITY_WSA
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(() =>
            {
                var fps = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.FAST |
                                                                     VuforiaRenderer.FpsHint.NO_VIDEOBACKGROUND);
                Application.targetFrameRate = fps;

                var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
                objectTracker.Stop();
                objectTracker.DeactivateDataSet(objectTracker.GetDataSets()
                    .First(set => set.Path == "Vuforia/Augmentix_Deskzone.xml"));
                objectTracker.Start();
            });
#endif
        }

        public void SetupDeskzoneAndConnect()
        {
#if UNITY_WSA
            if (!PhotonNetwork.IsConnected)
            {
                var target = _deskzone.transform.parent;
                _deskzone.transform.parent = null;
                target.gameObject.SetActive(false);
                var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
                objectTracker.Stop();
                objectTracker.DeactivateDataSet(objectTracker.GetDataSets()
                    .First(set => set.Path == "Vuforia/Augmentix_Floor.xml"));
                objectTracker.ActivateDataSet(objectTracker.GetDataSets()
                    .First(set => set.Path == "Vuforia/Augmentix_Deskzone.xml"));
                objectTracker.Start();
                _deskzone.gameObject.AddComponent<WorldAnchor>();
                Connect();
            }
#endif
        }
    }
}