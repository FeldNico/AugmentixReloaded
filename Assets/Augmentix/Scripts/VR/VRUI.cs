using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Augmentix.Scripts.OOI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Outline = Augmentix.Scripts.OOI.Outline;

namespace Augmentix.Scripts.VR
{
    public class VRUI : MonoBehaviour, IOnEventCallback
    {
        public GameObject IndicationPrefab;
        public float HighlightDistance;

        private GameObject _currentTarget;
        private GameObject _indicator;
        private Coroutine _indicatorRotate;
        
        
        
        public void ToggleHighlightTarget(GameObject Target)
        {
            if (_currentTarget != null && _currentTarget != Target)
            {
                _currentTarget.GetComponent<Outline>().enabled = false;
                var outline = Target.GetComponent<Outline>();
                if (!outline)
                {
                    outline = Target.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineVisible;
                }
                outline.enabled = true;
                _currentTarget = Target;
                
                return;
            }

            if (_currentTarget == null)
            {
                if (_indicator == null)
                {
                    _indicator = Instantiate(IndicationPrefab,Vector3.zero, Quaternion.Euler(80, 0, 0),
                        Camera.main.transform);
                    _indicator.transform.localPosition = new Vector3(0,-0.15f,0.5f);
                    _indicator.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                }

                _currentTarget = Target;
                var outline = _currentTarget.GetComponent<Outline>();
                if (!outline)
                {
                    outline = _currentTarget.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineVisible;
                }
                outline.enabled = true;
                _indicator.SetActive(true);
                _indicatorRotate = StartCoroutine(RotateIndicator());
            }
            else
            {
                _indicator.SetActive(false);
                _currentTarget.GetComponent<Outline>().enabled = false;
                _currentTarget = null;
                try
                {
                    StopCoroutine(_indicatorRotate);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            IEnumerator RotateIndicator()
            {
                var camTransform = Camera.main.transform;
                while (true)
                {
                    var ooi = _currentTarget.GetComponent<OOI.OOI>();
                    var playerPos = camTransform.position;
                    var closedPoint = ooi.Collider.ClosestPoint(playerPos);
                    var indicatorTransform = _indicator.transform;
                    indicatorTransform.LookAt(closedPoint);
                    if ( closedPoint == playerPos || (Quaternion.Angle(indicatorTransform.rotation, Camera.main.transform.rotation) < 30f && Vector3.Distance(closedPoint, playerPos) < HighlightDistance))
                    {
                        _indicator.gameObject.SetActive(false);
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case (byte) TargetManager.EventCode.HIGHLIGHT:
                {
                    ToggleHighlightTarget(PhotonView.Find((int)photonEvent.CustomData).gameObject);
                    break;
                }
            }
        }
        
        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}