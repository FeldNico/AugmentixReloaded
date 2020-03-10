using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using UnityEngine;
using UnityEngine.UIElements;

public class LMPinchPoseMessage : ILMMessage
{
    public const LMProtocol.LeapMotionMessageType Type = LMProtocol.LeapMotionMessageType.PinchPose;
    public bool IsRight;
    public float PinchStrength;
    public Quaternion PalmRotation;
    public Vector3 PalmPosition;
    
    public int ConvertFromBytes(byte[] data, int startIndex)
    {
        IsRight = data[startIndex] == 0x0;
        startIndex += sizeof(byte);
        PinchStrength = BitConverter.ToSingle(data, startIndex + 0 * sizeof(float));
        PalmRotation.w = BitConverter.ToSingle(data, startIndex + 1 * sizeof(float));
        PalmRotation.x = BitConverter.ToSingle(data, startIndex + 2 * sizeof(float));
        PalmRotation.y = BitConverter.ToSingle(data, startIndex + 3 * sizeof(float));
        PalmRotation.z = BitConverter.ToSingle(data, startIndex + 4 * sizeof(float));
        PalmPosition.x = BitConverter.ToSingle(data, startIndex + 5 * sizeof(float));
        PalmPosition.y = BitConverter.ToSingle(data, startIndex + 6 * sizeof(float));
        PalmPosition.z = BitConverter.ToSingle(data, startIndex + 7 * sizeof(float));
        return startIndex + 8 * sizeof(float);
    }

    public byte[] ConvertToBytes()
    {
        byte[] data = new byte[2*sizeof(byte)+8*sizeof(float)];
        data[0] = (byte) Type;
        data[1] = (byte) (IsRight ? 0x0 : 0x1);
        Buffer.BlockCopy(BitConverter.GetBytes(PinchStrength), 0, data, 2*sizeof(byte) + 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmRotation.w), 0, data, 2*sizeof(byte) + 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmRotation.x), 0, data, 2*sizeof(byte) + 2 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmRotation.y), 0, data, 2*sizeof(byte) + 3 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmRotation.z), 0, data, 2*sizeof(byte) + 4 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.x), 0, data, 2*sizeof(byte) + 5 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.y), 0, data, 2*sizeof(byte) + 6 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PalmPosition.z), 0, data, 2*sizeof(byte) + 7 * sizeof(float), sizeof(float));
        return data;
    }

    public void HandleMessage()
    {
#if UNITY_WSA
        var hands = ((ARTargetManager) TargetManager.Instance).Hands;
        var hand = IsRight ? hands.Right : hands.Left;
        hand.PinchStrength = PinchStrength;
        hand.Palm.transform.localRotation = PalmRotation;
        hand.Palm.transform.localPosition = PalmPosition;
        hand.PinchingSphere.gameObject.SetActive(true);
        if (!hand.IsPinching && PinchStrength > 0.9F)
        {
            hand.IsPinching = true;
            hand.OnPinchStart?.Invoke();
        }

        if (hand.IsPinching && PinchStrength <= 0.9F)
        {
            hand.IsPinching = false;
            hand.OnPinchEnd?.Invoke();
        }
#endif   
    }

    public void OnTimeout()
    {
#if UNITY_WSA
        var hands = ((ARTargetManager) TargetManager.Instance).Hands;
        var hand = IsRight ? hands.Right : hands.Left;
        hand.PinchingSphere.gameObject.SetActive(true);
        if (hand.IsPinching)
        {
            hand.IsPinching = false;
            hand.OnPinchEnd?.Invoke();
        }
#endif
    }

    public void HandleTimeout(List<LMProtocol.TimeOutData> cache)
    {
        foreach (var outData in cache)
        {
            if (outData.Type == Type && ((LMPinchPoseMessage) outData.Message).IsRight == IsRight)
            {
                outData.Time = 0;
                return;
            }
        }

        var msg = (LMPinchPoseMessage) Activator.CreateInstance(typeof(LMPinchPoseMessage));
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
