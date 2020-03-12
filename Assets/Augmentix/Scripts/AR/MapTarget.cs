using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts.Network;
using Photon.Pun;
using UnityEngine;

namespace Augmentix.Scripts.AR
{
    public class MapTarget : MonoBehaviour
    {
        public float PlayerScale;
        public float Scale;
        public Vector3 MapOffset = new Vector3(1.694f,0f,1.261f);
        public GameObject Scaler { private set; get; }
        public Dictionary<PlayerAvatar,GameObject> AvatarDummies { private set; get; } = new Dictionary<PlayerAvatar, GameObject>();
        public Dictionary<Warpzone,GameObject> WarpzoneDummies { private set; get; } = new Dictionary<Warpzone, GameObject>();
        public List<MapTargetDummy> Dummies { private set; get; } = new List<MapTargetDummy>();
        
        private ARTargetManager _targetManager;
        private VirtualCity _virtualCity;

        private void Start()
        {
            _targetManager = FindObjectOfType<ARTargetManager>();
            _virtualCity = FindObjectOfType<VirtualCity>();

            Scaler = new GameObject("Scaler");
            Scaler.transform.parent = transform;
            Scaler.transform.localPosition = MapOffset;
            Scaler.transform.localScale = new Vector3(Scale,Scale,Scale);

            PlayerAvatar.AvatarCreated += (avatar) =>
            {
                if (avatar.GetComponent<PhotonView>().IsMine)
                    return;

                var dummy = Instantiate(_targetManager.AvatarPrefab,Scaler.transform);
                Destroy(dummy.GetComponent<AugmentixTransformView>());
                Destroy(dummy.GetComponent<PlayerAvatar>());
                Destroy(dummy.GetComponent<PhotonView>());
                var dummyComp = dummy.gameObject.AddComponent<MapTargetDummy>();
                dummyComp.Target = avatar;
                Dummies.Add(dummyComp);
                dummy.transform.localPosition =
                    _virtualCity.transform.InverseTransformPoint(avatar.transform.position);
                dummy.transform.localRotation =
                    Quaternion.Inverse(_virtualCity.transform.rotation) * avatar.transform.rotation;
                dummy.transform.localScale *= PlayerScale;
            };

            PlayerAvatar.AvatarLost += (avatar) =>
            {
                for (var i = 0; i < Dummies.Count; i++)
                {
                    var dummy = Dummies[i];
                    if (dummy.Target == avatar)
                    {
                        Dummies.Remove(dummy);
                        return;
                    }
                }
            };

            StartCoroutine(OnLateStart());
            IEnumerator OnLateStart()
            {
                yield return new WaitForEndOfFrame();
                foreach (var warpzone in FindObjectsOfType<Warpzone>())
                {
                    var dummy = Instantiate(_targetManager.WarpzoneDummyPrefab, Scaler.transform);
                    var dummyComp = dummy.gameObject.AddComponent<MapTargetDummy>();
                    dummyComp.Target = warpzone;
                    Dummies.Add(dummyComp);
                    dummy.transform.localPosition =
                        warpzone.LocalPosition;
                    dummy.transform.localRotation = Quaternion.identity;
                    dummy.transform.localScale *= 1f/warpzone.Scale;
                }
            }
        }

        void Update()
        {
            foreach (var dummy in Dummies)
            {
                var target = dummy.Target;
                
                if (target is PlayerAvatar)
                {
                    dummy.transform.localPosition = _virtualCity.transform.InverseTransformPoint(target.transform.position);
                    dummy.transform.localRotation =
                        Quaternion.Inverse(_virtualCity.transform.rotation) * target.transform.rotation;
                } else if (target is Warpzone)
                {
                    dummy.transform.localPosition = ((Warpzone) target).LocalPosition;
                }
            }
        }
    }
}
