using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Augmentix.Scripts;
using Augmentix.Scripts.AR;
using UnityEngine;

public class LMPointingMessage : ILMMessage
{
    public const LMProtocol.LeapMotionMessageType Type = LMProtocol.LeapMotionMessageType.Pointing;
    public bool IsRight;
    public Vector3 IndexPosition;
    public Vector3 Direction;
    
    public int ConvertFromBytes(byte[] data, int startIndex)
    {
        IsRight = data[startIndex] == 0x0;
        startIndex += sizeof(byte);
        IndexPosition.x = BitConverter.ToSingle(data, startIndex + 0 * sizeof(float));
        IndexPosition.y = BitConverter.ToSingle(data, startIndex + 1 * sizeof(float));
        IndexPosition.z = BitConverter.ToSingle(data, startIndex + 2 * sizeof(float));
        Direction.x = BitConverter.ToSingle(data, startIndex + 3 * sizeof(float));
        Direction.y = BitConverter.ToSingle(data, startIndex + 4 * sizeof(float));
        Direction.z = BitConverter.ToSingle(data, startIndex + 5 * sizeof(float));
        return startIndex + 6 * sizeof(float);
    }

    public byte[] ConvertToBytes()
    {
        byte[] data = new byte[2*sizeof(byte)+6*sizeof(float)];
        data[0] = (byte) Type;
        data[1] = (byte) (IsRight ? 0x0 : 0x1);
        Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.x), 0, data, 2*sizeof(byte) + 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.y), 0, data, 2*sizeof(byte) + 1 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(IndexPosition.z), 0, data, 2*sizeof(byte) + 2 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Direction.x), 0, data, 2*sizeof(byte) + 3 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Direction.y), 0, data, 2*sizeof(byte) + 4 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(Direction.z), 0, data, 2*sizeof(byte) + 5 * sizeof(float), sizeof(float));
        return data;
    }

    public void HandleMessage()
    {
#if UNITY_WSA
        var hands = ((ARTargetManager) TargetManager.Instance).Hands;
        var hand = IsRight ? hands.Right : hands.Left;
        hand.Index.transform.localPosition = IndexPosition;
        hand.PointingDirection = Direction;
        if (!hand.IsPointing)
        {
            hand.IsPointing = true;
            hand.OnPointStart?.Invoke();
        }
#endif   
    }

    public void OnTimeout()
    {
#if UNITY_WSA
        var hands = ((ARTargetManager) TargetManager.Instance).Hands;
        var hand = IsRight ? hands.Right : hands.Left;
        if (hand.IsPointing)
        {
            hand.IsPointing = false;
            hand.OnPointEnd?.Invoke();
        }
#endif
    }

    public void HandleTimeout(List<LMProtocol.TimeOutData> cache)
    {
        foreach (var outData in cache)
        {
            if (outData.Type == Type && ((LMPointingMessage) outData.Message).IsRight == IsRight)
            {
                outData.Time = 0;
                return;
            }
        }

        var msg = (LMPointingMessage) Activator.CreateInstance(typeof(LMPointingMessage));
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
