using System.Collections;
using System.Collections.Generic;
using UnityEngine;

        
public interface ILMMessage
{
    int ConvertFromBytes(byte[] data,int startIndex);
    byte[] ConvertToBytes();
    void HandleMessage();
}