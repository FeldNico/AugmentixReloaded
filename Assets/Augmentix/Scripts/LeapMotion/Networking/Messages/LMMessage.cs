using System.Collections;
using System.Collections.Generic;
using UnityEngine;

        
public interface ILMMessage
{
    int ConvertFromBytes(byte[] data,int startIndex);
    byte[] ConvertToBytes();
    void HandleMessage();
    void OnTimeout();
    void HandleTimeout(List<LMProtocol.TimeOutData> cache);
    LMProtocol.LeapMotionMessageType GetMessageType();
}