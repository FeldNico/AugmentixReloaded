using System;
using System.Collections.Generic;
using Augmentix.Scripts.AR;
#if UNITY_WSA || UNITY_STANDALONE_WIN
using Leap.Unity;
using UnityEngine;

namespace Augmentix.Scripts.LeapMotion.Networking.Messages
{
    public class LMUpdateMessage : ILMMessage
    {
        public const LMProtocol.LeapMotionMessageType Type = LMProtocol.LeapMotionMessageType.Update;
        public bool IsRight;
        public float PinchStrength;
        public Vector3 ThumbPosition;
        public Vector3 IndexPosition;
        public Vector3 PalmPosition;

        public int ConvertFromBytes(byte[] data, int startIndex)
        {
            IsRight = data[startIndex] == 0x0;
            startIndex += sizeof(byte);
            PinchStrength = BitConverter.ToSingle(data, startIndex);
            startIndex += sizeof(float);
            ThumbPosition.x = BitConverter.ToSingle(data, startIndex + 0 * sizeof(float));
            ThumbPosition.y = BitConverter.ToSingle(data, startIndex + 1 * sizeof(float));
            ThumbPosition.z = BitConverter.ToSingle(data, startIndex + 2 * sizeof(float));
            IndexPosition.x = BitConverter.ToSingle(data, startIndex + 3 * sizeof(float));
            IndexPosition.y = BitConverter.ToSingle(data, startIndex + 4 * sizeof(float));
            IndexPosition.z = BitConverter.ToSingle(data, startIndex + 5 * sizeof(float));
            PalmPosition.x = BitConverter.ToSingle(data, startIndex + 6 * sizeof(float));
            PalmPosition.y = BitConverter.ToSingle(data, startIndex + 7 * sizeof(float));
            PalmPosition.z = BitConverter.ToSingle(data, startIndex + 8 * sizeof(float));
            return startIndex + 9 * sizeof(float);
        }

        public byte[] ConvertToBytes()
        {
            byte[] data = new byte[2*sizeof(byte)+10*sizeof(float)];
            data[0] = (byte) Type;
            data[1] = (byte) (IsRight ? 0x0 : 0x1);
            Buffer.BlockCopy(BitConverter.GetBytes(PinchStrength), 0, data, 2*sizeof(byte) + 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(ThumbPosition.x), 0, data, 2*sizeof(byte) + 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(ThumbPosition.y), 0, data, 2*sizeof(byte) + 2 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(ThumbPosition.z), 0, data, 2*sizeof(byte) + 3 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.x), 0, data, 2*sizeof(byte) + 4 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.y), 0, data, 2*sizeof(byte) + 5 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.z), 0, data, 2*sizeof(byte) + 6 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.x), 0, data, 2*sizeof(byte) + 7 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.y), 0, data, 2*sizeof(byte) + 8 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.z), 0, data, 2*sizeof(byte) + 9 * sizeof(float), sizeof(float));
            return data;
        }
        
        public void HandleMessage()
        {
#if UNITY_WSA
            var hands = ((ARTargetManager) TargetManager.Instance).Hands;
            var hand = IsRight ? hands.Right : hands.Left;
            hand.Thumb.transform.localPosition = ThumbPosition;
            hand.Index.transform.localPosition = IndexPosition;
            hand.Palm.transform.localPosition = PalmPosition;
            hand.PinchStrength = PinchStrength;
            if (!hand.IsDetected)
            {
                hand.IsDetected = true;
                hand.OnDetect?.Invoke();
            }
#endif
        }

        public void OnTimeout()
        {
#if UNITY_WSA
            var hands = ((ARTargetManager) TargetManager.Instance).Hands;
            var hand = IsRight ? hands.Right : hands.Left;
            if (hand.IsDetected)
            {
                hand.IsDetected = false;
                hand.OnLost?.Invoke();
            }
#endif
        }

        public void HandleTimeout(List<LMProtocol.TimeOutData> cache)
        {
            foreach (var outData in cache)
            {
                if (outData.Type == Type && ((LMUpdateMessage) outData.Message).IsRight == IsRight)
                {
                    outData.Time = 0;
                    return;
                }
            }
            
            var msg = (LMUpdateMessage) Activator.CreateInstance(typeof(LMUpdateMessage));
            msg.IsRight = IsRight;
            var data = new LMProtocol.TimeOutData
            {
                Message = msg,
                Time = 0,
                Type = Type
            };
            cache.Add(data);
        }
    }
}
#endif