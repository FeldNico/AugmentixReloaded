using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Photon.Pun;
using UnityEngine;

public abstract class LMProtocol: MonoBehaviour
{
    public static LMProtocol Instance { private set; get; }
    public static Dictionary<LeapMotionMessageType,Type> TypeDict { get; } = new Dictionary<LeapMotionMessageType, Type>();
    
    public int BufferSize = 4 * 64;
    public int UpdateFrequenz = 0;

    public enum LeapMotionMessageType : byte
    {
        Invalid = 0x0, //DO NOT USE!
        Update = 0x1,
        Detected = 0x2,
        Pointing = 0x3,
    }
    
    public class TimeOutData
    {
        public LeapMotionMessageType Type;
        public ILMMessage Message;
        public int Time;
    }
    
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    public class State
    {
        public byte[] buffer;

        public State(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }
    }
#endif

    private int _currentFrame = 0;
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
    
    void Awake()
    {
        Instance = this;
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

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

}
