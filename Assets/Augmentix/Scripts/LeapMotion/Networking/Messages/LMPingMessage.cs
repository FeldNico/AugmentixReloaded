using System;
using System.Collections.Generic;
using Augmentix.Scripts.AR;
#if UNITY_WSA || UNITY_STANDALONE_WIN
using Leap.Unity;
using UnityEngine;

namespace Augmentix.Scripts.LeapMotion.Networking.Messages
{
    public class LMPingMessage : ILMMessage
    {
        public const LMProtocol.LeapMotionMessageType Type = LMProtocol.LeapMotionMessageType.Ping;
        public bool IsRight;

        public int ConvertFromBytes(byte[] data, int startIndex)
        {
            IsRight = data[startIndex] == 0x0;
            return startIndex +  sizeof(byte);
        }

        public byte[] ConvertToBytes()
        {
            byte[] data = new byte[2*sizeof(byte)];
            data[0] = (byte) Type;
            data[1] = (byte) (IsRight ? 0x0 : 0x1);
            return data;
        }
        
        public void HandleMessage()
        {
#if UNITY_WSA
            var hands = ((ARTargetManager) TargetManager.Instance).Hands;
            var hand = IsRight ? hands.Right : hands.Left;
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
                if (outData.Type == Type && ((LMPingMessage) outData.Message).IsRight == IsRight)
                {
                    outData.Time = 0;
                    return;
                }
            }
            
            var msg = (LMPingMessage) Activator.CreateInstance(typeof(LMPingMessage));
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