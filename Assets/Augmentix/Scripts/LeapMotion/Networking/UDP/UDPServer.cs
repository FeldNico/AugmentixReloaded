using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
#if UNITY_WSA && !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif
using UnityEngine;

public class UDPServer
{
    public int Port { private set; get; }

    private string _ip;

    public string IP
    {
        get
        {
            if (_ip == null)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _ip = ip.ToString();
                        break;
                    }
                }
            }
            return _ip;
        }
    }

    private int _currentFrame = 0;

#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    private Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private LMProtocol.State state = new LMProtocol.State();
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv;
#elif UNITY_WSA && !UNITY_EDITOR
        private DatagramSocket _server;
#endif
    //private ConcurrentStack<byte[]> _dataStack = new ConcurrentStack<byte[]>();
    private ConcurrentStack<byte[]> _messageStack = new ConcurrentStack<byte[]>();

    public UDPServer(int port)
    {
        Port = port;
    }

    public async void Connect()
    {
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
        _server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _server.Bind(new IPEndPoint(IPAddress.Any, Port));

        _server.BeginReceiveFrom(state.buffer, 0, LMProtocol.BUFSIZE, SocketFlags.None, ref epFrom, recv = (ar) =>
        {
            LMProtocol.State so = (LMProtocol.State) ar.AsyncState;
            _server.EndReceiveFrom(ar, ref epFrom);
            _server.BeginReceiveFrom(so.buffer, 0, LMProtocol.BUFSIZE, SocketFlags.None, ref epFrom, recv, so);
            _messageStack.Push(so.buffer);
        }, state);

#elif UNITY_WSA && !UNITY_EDITOR
        _server = new DatagramSocket();
        _server.MessageReceived += (sender, args) => { 
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            MemoryStream ms = ToMemoryStream(streamIn);
            _messageStack.Push(ms.ToArray());
        };
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            HostName IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().SingleOrDefault(hn =>
                hn.IPInformation?.NetworkAdapter != null 
                    && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);
            await _server.BindEndpointAsync(IP,Port.ToString());
            Debug.Log("Bind Server "+IP+":"+Port);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
            return;
        }
#endif
        Debug.Log("Server started");
    }

    private MemoryStream ToMemoryStream(Stream input)
    {
        try
        {
            // Read and write in
            byte[] block = new byte[512]; // blocks of 4K.
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                int bytesRead = input.Read(block, 0, block.Length);
                if (bytesRead == 0) return ms;
                ms.Write(block, 0, bytesRead);
            }
        }
        finally
        {
        }
    }

    public byte[] GetCurrentMessageArray()
    {
        var bytes = new byte[0];
        if (_messageStack.TryPop(out bytes))
        {
            _messageStack.Clear();
            return bytes;
        }
        return null;
    }

    public bool ContainsMessage()
    {
        return _messageStack.Count != 0;
    }
    
}