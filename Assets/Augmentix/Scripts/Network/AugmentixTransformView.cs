using Microsoft.MixedReality.Toolkit;
using Photon.Pun;
using UnityEngine;

namespace Augmentix.Scripts.Network
{
    [RequireComponent(typeof(PhotonView))]
    public class AugmentixTransformView : MonoBehaviour, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private PhotonView m_PhotonView;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;
        
        public bool m_SynchronizeScale = true;

        bool m_firstTake = false;

        private Transform _virtualCityTransform;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();
            _virtualCityTransform = FindObjectOfType<VirtualCity>().transform;
            
            m_StoredPosition = _virtualCityTransform.InverseTransformPoint(transform.position) ;
            m_NetworkPosition = Vector3.zero;
            m_NetworkRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (!this.m_PhotonView.IsMine)
            {
                transform.position = Vector3.MoveTowards(transform.position, _virtualCityTransform.TransformPoint(this.m_NetworkPosition), this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _virtualCityTransform.rotation * this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                var newPosition = _virtualCityTransform.InverseTransformPoint(transform.position);
                    
                this.m_Direction = newPosition - this.m_StoredPosition;
                this.m_StoredPosition = newPosition;

                stream.SendNext(newPosition);
                stream.SendNext(this.m_Direction);

                stream.SendNext(Quaternion.Inverse(_virtualCityTransform.rotation) *  transform.rotation);

                if (this.m_SynchronizeScale)
                {
                    Vector3 transformedSize = transform.lossyScale;

                    Transform t = _virtualCityTransform;
                    Vector3 localScale;
                    do
                    {
                        localScale = t.localScale;
                        transformedSize.x /= localScale.x;
                        transformedSize.y /= localScale.y;
                        transformedSize.z /= localScale.z;
                        t = t.parent;
                    }
                    while (t != null);
                    
                    stream.SendNext(transformedSize);
                }
            }
            else
            {
                
                this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
                this.m_Direction = (Vector3)stream.ReceiveNext();

                if (m_firstTake)
                {
                    transform.position = _virtualCityTransform.TransformPoint(this.m_NetworkPosition);
                    this.m_Distance = 0f;
                }
                else
                {
                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    this.m_NetworkPosition += this.m_Direction * lag;
                    this.m_Distance = Vector3.Distance(transform.position, this._virtualCityTransform.TransformPoint(m_NetworkPosition));
                }

                this.m_NetworkRotation =  (Quaternion)stream.ReceiveNext();

                if (m_firstTake)
                {
                    this.m_Angle = 0f;
                    transform.localRotation = _virtualCityTransform.rotation *this.m_NetworkRotation;
                }
                else
                {
                    this.m_Angle = Quaternion.Angle(transform.rotation, _virtualCityTransform.rotation * this.m_NetworkRotation);
                }

                if (this.m_SynchronizeScale)
                {
                    Vector3 transformedSize = (Vector3)stream.ReceiveNext();

                    Transform t = _virtualCityTransform;
                    Vector3 localScale;
                    do
                    {
                        localScale = t.localScale;
                        transformedSize.x *= localScale.x;
                        transformedSize.y *= localScale.y;
                        transformedSize.z *= localScale.z;
                        t = t.parent;
                    }
                    while (t != null);

                    var parent = transform.parent;
                    transform.parent = null;
                    transform.localScale = transformedSize;
                    transform.parent = parent;
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}