using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
using System.Net;
using System.Net.Sockets;
#endif
using UnityEngine;

public class UDPClient
{
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    private Socket _socket { get; } = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private LMProtocol.State state = new LMProtocol.State();
#endif

    public void Connect(string ip, int port)
    {
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
        _socket.Connect(ip,port);
#endif
    }

    public void Send<T>(T data, Func<T,byte[]> serializeFunc)
    {
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
        byte[] bytes = serializeFunc(data);
        _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, (ar) =>
        {
            _socket.EndSend(ar);
        }, state);
#endif
    }
    
}
