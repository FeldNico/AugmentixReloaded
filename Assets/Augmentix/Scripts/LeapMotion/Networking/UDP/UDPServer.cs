using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

    private byte[] _currentMessage;
    public byte[] CurrentMessage
    {
        get
        {
            /*
            rwl.AcquireReaderLock(-1);
            try
            {
                var tmp = _currentMessage;
                _currentMessage = null;
                return tmp;
            }
            finally
            {
                rwl.ReleaseReaderLock();
            }
            */
            /*
            var tmp = _currentMessage;
            _currentMessage = null;
            return tmp;
            */
            return _currentMessage;
        }
        private set
        {
            /*
            rwl.AcquireWriterLock(-1);
            try
            {
                _currentMessage = value;
            }
            finally
            {
                rwl.ReleaseWriterLock();
            }
            */
            _currentMessage = value;
        }
    }

    private int _currentFrame = 0;
    static ReaderWriterLock rwl = new ReaderWriterLock();

#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    private Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private LMProtocol.State state;
    private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback recv;
#elif UNITY_WSA && !UNITY_EDITOR
        private DatagramSocket _server;
#endif

    public UDPServer(int port)
    {
        Port = port;
    }

    private byte[] _block;
    public async void Connect()
    {
#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
        state = new LMProtocol.State(LMProtocol.Instance.BufferSize);
        _server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _server.Bind(new IPEndPoint(IPAddress.Any, Port));
        _server.BeginReceiveFrom(state.buffer, 0, LMProtocol.Instance.BufferSize, SocketFlags.None, ref epFrom, recv = (ar) =>
        {
            LMProtocol.State so = (LMProtocol.State) ar.AsyncState;
            _server.EndReceiveFrom(ar, ref epFrom);
            _server.BeginReceiveFrom(so.buffer, 0, LMProtocol.Instance.BufferSize, SocketFlags.None, ref epFrom, recv, so);
            CurrentMessage = (byte[]) so.buffer.Clone();
            Array.Clear(so.buffer, 0, so.buffer.Length);
        }, state);
        
#elif UNITY_WSA && !UNITY_EDITOR
        _block = new byte[LMProtocol.Instance.BufferSize];
        _server = new DatagramSocket();
        _server.Control.QualityOfService = SocketQualityOfService.LowLatency;
        _server.Control.InboundBufferSizeInBytes = ((uint) LMProtocol.Instance.BufferSize)*2048u;
        _server.MessageReceived += (sender, args) => { 

/*
            using (var streamIn = args.GetDataStream().AsStreamForRead())
            {
                Array.Clear(_block, 0, _block.Length);
                streamIn.Read(_block,0,LMProtocol.Instance.BufferSize);
                CurrentMessage = _block;
            }
*/
            Array.Clear(_block, 0, _block.Length);
            using (var reader = args.GetDataReader())
            {
                Array.Resize(ref _block,(int)reader.UnconsumedBufferLength);
                reader.ReadBytes(_block);
            }
            CurrentMessage = _block;
/*
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            Array.Clear(_block, 0, _block.Length);
            streamIn.Read(_block,0,LMProtocol.Instance.BufferSize);
            CurrentMessage = _block;
            streamIn.Flush();
*/
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

}