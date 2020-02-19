using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_WSA
using Augmentix.Scripts.LeapMotion;
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
        public int Port = 1337;
        public ARHands Hands;
        public GameObject AvatarPrefab;
        [HideInInspector]
        public bool DoCalibrate = false;
        [HideInInspector]
        public Vector3 FirstCalibrationVector, SecondCalibrationVector;
        public TMP_Text DebugText;


        private WarpzoneManager _warpzoneManager;
        private VirtualCity _virtualCity;

        new public void Awake()
        {
            if (Hands == null)
                Hands = FindObjectOfType<ARHands>();
            if (DebugText == null)
                DebugText = FindObjectOfType<TMP_Text>();

            _warpzoneManager = FindObjectOfType<WarpzoneManager>();
            _virtualCity = FindObjectOfType<VirtualCity>();
            
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
                PhotonNetwork.SetInterestGroups((byte) Groups.LEAP_MOTION, true);
                
                var avatar = PhotonNetwork.Instantiate(AvatarPrefab != null ? AvatarPrefab.name : "Primary_Avatar", Camera.main.transform.position,
                    Camera.main.transform.rotation);
                avatar.transform.parent = Camera.main.transform;
                avatar.GetComponent<Renderer>().enabled = false;
            };
        }

        public void SetupDeskzoneAndConnect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                FindObjectOfType<VirtualCity>().transform.parent = null;
                FindObjectOfType<Deskzone>().transform.parent = null;
                Connect();
            }
        }

        private string[] axisList = new[]
        {
            "Horizontal", "Vertical", "Fire1", "Fire2", "Fire3", "Jump", "Mouse X", "Mouse Y", "Mouse ScrollWheel",
            "Horizontal", "Vertical", "Fire1", "Fire2", "Fire3", "Jump", "Submit", "Submit", "Cancel", "AXIS_1",
            "AXIS_2", "AXIS_3", "AXIS_4", "AXIS_5", "AXIS_6", "AXIS_7", "AXIS_8", "AXIS_9", "AXIS_10", "AXIS_11",
            "AXIS_12", "AXIS_13", "AXIS_14", "AXIS_15", "AXIS_16", "AXIS_17", "AXIS_18", "AXIS_19", "AXIS_20",
            "AXIS_21", "AXIS_22", "AXIS_23", "AXIS_24", "AXIS_25", "AXIS_26", "AXIS_27", "AXIS_28", "UpDown", "UpDown",
            "Oculus_GearVR_LThumbstickX", "Oculus_GearVR_LThumbstickY", "Oculus_GearVR_RThumbstickX",
            "Oculus_GearVR_RThumbstickY", "Oculus_GearVR_DpadX", "Oculus_GearVR_DpadY", "Oculus_GearVR_LIndexTrigger",
            "Oculus_GearVR_RIndexTrigger", "Oculus_CrossPlatform_Button2", "Oculus_CrossPlatform_Button4",
            "Oculus_CrossPlatform_PrimaryThumbstick", "Oculus_CrossPlatform_SecondaryThumbstick",
            "Oculus_CrossPlatform_PrimaryIndexTrigger", "Oculus_CrossPlatform_SecondaryIndexTrigger",
            "Oculus_CrossPlatform_PrimaryHandTrigger", "Oculus_CrossPlatform_SecondaryHandTrigger",
            "Oculus_CrossPlatform_PrimaryThumbstickHorizontal", "Oculus_CrossPlatform_PrimaryThumbstickVertical",
            "Oculus_CrossPlatform_SecondaryThumbstickHorizontal", "Oculus_CrossPlatform_SecondaryThumbstickVertical"
        };
        
        public void Update()
        {
            var x = Input.GetAxis("Horizontal");
            var y = Input.GetAxis("Vertical");
            
            if (Math.Abs(x) > 0.2f || Math.Abs(y) > 0.2f)
            {
                var activeWarpzone = _warpzoneManager.ActiveWarpzone;
                var vec = new Vector3();
                if (activeWarpzone != null)
                {
                    if (Math.Abs(x) > 0.2f)
                    {
                        vec.x = Math.Sign(x) * _warpzoneManager.ScrollSpeed * activeWarpzone .Scale;
                    }
                    if (Math.Abs(y) > 0.2f)
                    {
                        vec.z = Math.Sign(y) * _warpzoneManager.ScrollSpeed * activeWarpzone.Scale;
                    }

                    activeWarpzone.Position += vec;
                }
                else
                {

                    if (Math.Abs(x) > 0.2f)
                    {
                        vec += 0.05f * Math.Sign(x) * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * Camera.main.transform.forward;
                    }
                    if (Math.Abs(y) > 0.2f)
                    {
                        vec += 0.05f * Math.Sign(x) * _warpzoneManager.ScrollSpeed * _virtualCity.transform.localScale.x * Camera.main.transform.right;
                    }

                    _virtualCity.transform.localPosition += vec;
                }
            }
        }
#endif

    }
}