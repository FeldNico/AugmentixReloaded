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

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;

        bool m_firstTake = false;

        private Transform _virtualCityTransform;

        public void Awake()
        {
            m_PhotonView = GetComponent<PhotonView>();

            m_StoredPosition = transform.position;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;

            _virtualCityTransform = FindObjectOfType<VirtualCity>().transform;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (!this.m_PhotonView.IsMine)
            {
                transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (this.m_SynchronizePosition)
                {
                    var newPosition = _virtualCityTransform.InverseTransformPoint(transform.position);
                    
                    this.m_Direction = newPosition - this.m_StoredPosition;
                    this.m_StoredPosition = newPosition;

                    stream.SendNext(newPosition);
                    stream.SendNext(this.m_Direction);
                }

                if (this.m_SynchronizeRotation)
                {
                    stream.SendNext(Quaternion.Inverse(_virtualCityTransform.rotation) *  transform.rotation);
                }

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
                
                if (this.m_SynchronizePosition)
                {
                    this.m_NetworkPosition = _virtualCityTransform.TransformPoint((Vector3)stream.ReceiveNext());
                    this.m_Direction = _virtualCityTransform.TransformVector((Vector3)stream.ReceiveNext());

                    if (m_firstTake)
                    {
                        transform.position = this.m_NetworkPosition;
                        this.m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        this.m_NetworkPosition += this.m_Direction * lag;
                        this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
                    }

                   
                }

                if (this.m_SynchronizeRotation)
                {
                    this.m_NetworkRotation = _virtualCityTransform.rotation * (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        this.m_Angle = 0f;
                        transform.localRotation = this.m_NetworkRotation;
                    }
                    else
                    {
                        this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);
                    }
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
                    transform.localScale = transformedSize;
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}