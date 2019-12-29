using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Photon.Pun;
using UnityEngine;

public abstract class LMProtocol: MonoBehaviour
{
    public const int BUFSIZE = 1 * 512;
    public const int UpdateFrequenz = 2;
    public static Dictionary<LeapMotionMessageType,Type> TypeDict { get; } = new Dictionary<LeapMotionMessageType, Type>();
    
    
    private int _currentFrame = 0;
    
    public enum LeapMotionMessageType : byte
    {
        Invalid = 0x0, //DO NOT USE!
        Update = 0x1,
        Detected = 0x2,
    }
    
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    public class State
    {
        public byte[] buffer = new byte[BUFSIZE];
    }
#endif

    
    void Awake()
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
            var type = (LeapMotionMessageType) data[index];
            if (type == LeapMotionMessageType.Invalid || !TypeDict.ContainsKey(type))
                break;
            index += sizeof(byte);
            
            var message = (ILMMessage) Activator.CreateInstance(TypeDict[type]);
            index = message.ConvertFromBytes(data, index);
            _messageList.Add(message);
        }
        return _messageList.ToArray();
    }
    
    public bool CheckUpdate()
    {
        if (_currentFrame >= UpdateFrequenz)
        {
            _currentFrame = 0;
            return true;
        }

        _currentFrame++;
        return false;
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
