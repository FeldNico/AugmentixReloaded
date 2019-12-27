using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class LMProtocol: MonoBehaviour
{
    public const int BUFSIZE = 12 * 1024;
    public const float UpdateFrequenz = 2;
    public static Dictionary<LeapMotionMessageType,Type> TypeDict { get; } = new Dictionary<LeapMotionMessageType, Type>();
    
    
    private int _currentFrame = 0;
    
    public enum LeapMotionMessageType : byte
    {
        PositionUpdate = 0x1,
        Detected = 0x2,
    }
    
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    public class State
    {
        public byte[] buffer = new byte[BUFSIZE];
    }
#endif

    protected void Indexing()
    {
        foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (type.GetInterfaces().Any(i => i == typeof(ILMMessage)) && type.GetFields().Any(info => info.FieldType == typeof(LeapMotionMessageType)))
                {
                    var t = (LeapMotionMessageType) type.GetFields().First(info => info.FieldType == typeof(LeapMotionMessageType)).GetRawConstantValue();
                    TypeDict[t] = type;
                    Debug.Log("Registered "+type+" for "+t);
                }
            }
        }
    }
    
    private static List<ILMMessage> _messageList = new List<ILMMessage>();
    public static ILMMessage[] ConvertBytesToMessageArray(byte[] data)
    {
        _messageList.Clear();
        var index = 0;
        while (index < data.Length)
        {
            var type = TypeDict[(LeapMotionMessageType) data[index++]];
            if (type == null) 
                break;
            
            var message = (ILMMessage) Activator.CreateInstance(type);
            index = message.ConvertFromBytes(data, index);
            _messageList.Add(message);
        }
        return _messageList.ToArray();
    }
    
    public bool CheckUpdate()
    {
        if (_currentFrame > 1f / LMProtocol.UpdateFrequenz)
        {
            _currentFrame = 0;
            return true;
        }

        _currentFrame++;
        return false;
    }

}
