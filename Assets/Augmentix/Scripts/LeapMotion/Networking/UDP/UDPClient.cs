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

    public void Send(byte[] data)
    {
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
        _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
        {
            _socket.EndSend(ar);
        }, state);
#endif
    }
    
}
