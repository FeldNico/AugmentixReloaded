﻿using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Leap;
using System;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public class UDPManager
{
    public const int BUFSIZE = 64 * 2014;
    public const float UpdateFrame = 0.1f;

#if UNITY_WSA && UNITY_EDITOR || UNITY_STANDALONE_WIN
    public class State
    {
        public byte[] buffer = new byte[BUFSIZE];
    }
#endif
}
